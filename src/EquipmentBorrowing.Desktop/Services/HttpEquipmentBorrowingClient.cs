using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.Services;

public sealed class HttpEquipmentBorrowingClient : IStudentRepository,
    IEquipmentRepository, IActiveBorrowingsReader, IBorrowingHistoryReader,
    IBorrowingSearchReader, IStudentManagementService, IEquipmentManagementService,
    IAtomicBorrowingService
{
    private readonly HttpClient _http;
    public HttpEquipmentBorrowingClient(HttpClient http) => _http = http;
    private sealed record StudentDto(int Id, string StudentNumber, string FullName, string? Contact, bool IsEligible)
    {
        public Student ToEntity() { var s = new Student(Id, StudentNumber, FullName, Contact); s.SetBorrowingEligibility(IsEligible); return s; }
    }
    private sealed record EquipmentDto(int Id, string EquipmentCode, string Name, string? Description, bool IsAvailable)
    {
        public Equipment ToEntity() { var e = new Equipment(Id, EquipmentCode, Name, Description); if (!IsAvailable) e.MarkAsBorrowed(); return e; }
    }
    private sealed record BorrowingDto(int Id, int StudentId, int EquipmentId, DateOnly BorrowedDate, DateOnly ExpectedReturnDate, DateOnly? ReturnedDate, string Status, string StudentNumber, string StudentName, string EquipmentCode, string EquipmentName)
    {
        public Borrowing ToEntity() => Borrowing.FromReadSnapshot(Id, new Student(StudentId, StudentNumber, StudentName), new Equipment(EquipmentId, EquipmentCode, EquipmentName), BorrowedDate, ExpectedReturnDate, ReturnedDate, Status);
    }
    private sealed record CheckoutDto(BorrowingDto[] Borrowings);
    private sealed record ErrorDto(string Code, string Message);
    private sealed class ApiFailure : Exception
    {
        public OperationErrorKind Kind { get; }
        public ApiFailure(OperationErrorKind kind, string message) : base(message) => Kind = kind;
    }
    private async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct, bool missingAllowed = false) where T : class
    {
        bool mutation = method != HttpMethod.Get;
        try
        {
            using var request = new HttpRequestMessage(method, path);
            if (body != null) request.Content = JsonContent.Create(body);
            using var response = await _http.SendAsync(request, ct);
            if (missingAllowed && response.StatusCode == HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode)
            {
                ErrorDto? error = null;
                try { error = await response.Content.ReadFromJsonAsync<ErrorDto>(cancellationToken: ct); } catch (System.Text.Json.JsonException) { } catch (NotSupportedException) { }
                var kind = (int)response.StatusCode switch { 400 => OperationErrorKind.Validation, 404 => OperationErrorKind.NotFound, 409 => OperationErrorKind.Conflict, 500 when error?.Code == "server_error" => OperationErrorKind.ServerError, _ => mutation ? OperationErrorKind.OutcomeUnknown : OperationErrorKind.ServerError };
                throw new ApiFailure(kind, kind == OperationErrorKind.OutcomeUnknown ? "Operation outcome unknown. Refresh records before retrying." : error?.Message ?? $"API returned HTTP {(int)response.StatusCode}.");
            }
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct) ?? throw new ApiFailure(mutation ? OperationErrorKind.OutcomeUnknown : OperationErrorKind.ServerError, "Invalid API response. Refresh before retrying.");
        }
        catch (ApiFailure) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or System.IO.IOException or System.Text.Json.JsonException or NotSupportedException)
        {
            throw new ApiFailure(mutation ? OperationErrorKind.OutcomeUnknown : OperationErrorKind.ServerError, mutation ? "No usable API response. Operation outcome unknown; refresh before retrying." : "Unable to read API response. Check that the API is running and refresh.");
        }
    }
    async Task<Student?> IStudentRepository.GetByIdAsync(int id, CancellationToken ct) => (await SendAsync<StudentDto>(HttpMethod.Get, $"students/{id}", null, ct, true))?.ToEntity();
    async Task<IReadOnlyList<Student>> IStudentRepository.GetAllAsync(CancellationToken ct) => (await SendAsync<StudentDto[]>(HttpMethod.Get, "students", null, ct))!.Select(x => x.ToEntity()).ToArray();
    async Task<Equipment?> IEquipmentRepository.GetByIdAsync(int id, CancellationToken ct) => (await SendAsync<EquipmentDto>(HttpMethod.Get, $"equipment/{id}", null, ct, true))?.ToEntity();
    async Task<IReadOnlyList<Equipment>> IEquipmentRepository.GetAllAsync(CancellationToken ct) => (await SendAsync<EquipmentDto[]>(HttpMethod.Get, "equipment", null, ct))!.Select(x => x.ToEntity()).ToArray();
    private async Task<IReadOnlyList<Borrowing>> ReadBorrowings(string path, CancellationToken ct) => (await SendAsync<BorrowingDto[]>(HttpMethod.Get, path, null, ct))!.Select(x => x.ToEntity()).ToArray();
    public Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default) => ReadBorrowings("borrowings?status=Active", cancellationToken);
    public Task<IReadOnlyList<Borrowing>> GetForStudentAsync(int studentId, CancellationToken cancellationToken = default) => ReadBorrowings($"students/{studentId}/borrowings", cancellationToken);
    public Task<IReadOnlyList<Borrowing>> GetForEquipmentAsync(int equipmentId, CancellationToken cancellationToken = default) => ReadBorrowings($"equipment/{equipmentId}/borrowings", cancellationToken);
    public Task<IReadOnlyList<Borrowing>> SearchAsync(BorrowingSearchCriteria criteria, CancellationToken cancellationToken = default) => ReadBorrowings($"borrowings?search={Uri.EscapeDataString(criteria.SearchText?.Trim() ?? "")}&status={criteria.Status}&sort={(criteria.SortField == BorrowingSortField.CheckoutDate ? "borrowedDate" : "expectedReturnDate")}&direction={(criteria.Direction == BorrowingSortDirection.Ascending ? "asc" : "desc")}", cancellationToken);
    private async Task<StudentOperationResult> StudentWrite(HttpMethod method, string path, object body, CancellationToken ct)
    {
        try { var dto = await SendAsync<StudentDto>(method, path, body, ct); return StudentOperationResult.Success(dto!.Id, "Student saved successfully."); }
        catch (ApiFailure e) { return e.Kind switch { OperationErrorKind.OutcomeUnknown => StudentOperationResult.Unknown() with { Message = e.Message }, OperationErrorKind.ServerError => StudentOperationResult.Failed(e.Message), _ => StudentOperationResult.Rejected(e.Message, e.Kind) }; }
    }
    public Task<StudentOperationResult> RegisterAsync(string studentNumber, string studentName, string? contactInformation, int? studentYear, CancellationToken cancellationToken = default) => studentYear.HasValue ? Task.FromResult(StudentOperationResult.Rejected("Year level is not supported by the API.")) : StudentWrite(HttpMethod.Post, "students", new { studentNumber, fullName = studentName, contact = contactInformation }, cancellationToken);
    public Task<StudentOperationResult> UpdateAsync(int studentId, string studentNumber, string studentName, string? contactInformation, int? studentYear, CancellationToken cancellationToken = default) => studentYear.HasValue ? Task.FromResult(StudentOperationResult.Rejected("Year level is not supported by the API.")) : UpdateProfileAsync(studentId, studentNumber, studentName, contactInformation, cancellationToken);
    public Task<StudentOperationResult> UpdateProfileAsync(int studentId, string studentNumber, string studentName, string? contactInformation, CancellationToken cancellationToken = default) => StudentWrite(HttpMethod.Put, $"students/{studentId}", new { studentNumber, fullName = studentName, contact = contactInformation }, cancellationToken);
    public Task<StudentOperationResult> SetEligibilityAsync(int studentId, bool isAllowedToBorrow, CancellationToken cancellationToken = default) => StudentWrite(HttpMethod.Patch, $"students/{studentId}/eligibility", new { isEligible = isAllowedToBorrow }, cancellationToken);
    private async Task<EquipmentOperationResult> EquipmentWrite(HttpMethod method, string path, object body, CancellationToken ct)
    {
        try { var dto = await SendAsync<EquipmentDto>(method, path, body, ct); return EquipmentOperationResult.Success(dto!.Id, "Equipment saved successfully."); }
        catch (ApiFailure e) { return e.Kind switch { OperationErrorKind.OutcomeUnknown => EquipmentOperationResult.Unknown() with { Message = e.Message }, OperationErrorKind.ServerError => EquipmentOperationResult.Failed(e.Message), _ => EquipmentOperationResult.Rejected(e.Message, e.Kind) }; }
    }
    Task<EquipmentOperationResult> IEquipmentManagementService.RegisterAsync(string equipmentIdentifier, string equipmentName, string? description, CancellationToken ct) => EquipmentWrite(HttpMethod.Post, "equipment", new { equipmentCode = equipmentIdentifier, name = equipmentName, description }, ct);
    Task<EquipmentOperationResult> IEquipmentManagementService.UpdateAsync(int equipmentId, string equipmentIdentifier, string equipmentName, string? description, CancellationToken ct) => EquipmentWrite(HttpMethod.Put, $"equipment/{equipmentId}", new { equipmentCode = equipmentIdentifier, name = equipmentName, description }, ct);
    private static BorrowingOperationResult Failure(ApiFailure e) => e.Kind switch { OperationErrorKind.OutcomeUnknown => BorrowingOperationResult.Unknown() with { Message = e.Message }, OperationErrorKind.ServerError => BorrowingOperationResult.Failed(e.Message), _ => BorrowingOperationResult.Rejected(e.Message, e.Kind) };
    public async Task<BorrowingOperationResult> CheckoutAsync(int studentId, IReadOnlyCollection<int> equipmentIds, DateOnly checkoutDate, DateOnly expectedReturnDate, CancellationToken cancellationToken = default)
    {
        try
        {
            if (equipmentIds.Count == 0) return BorrowingOperationResult.Rejected("Select equipment.");
            if (equipmentIds.Count == 1) { var dto = await SendAsync<BorrowingDto>(HttpMethod.Post, "borrowings", new { studentId, equipmentId = equipmentIds.Single(), expectedReturnDate }, cancellationToken); return BorrowingOperationResult.Success("Checkout successful.", new[] { dto!.Id }); }
            var result = await SendAsync<CheckoutDto>(HttpMethod.Post, "checkout-requests", new { studentId, equipmentIds, expectedReturnDate }, cancellationToken);
            return BorrowingOperationResult.Success("Checkout successful.", result!.Borrowings.Select(x => x.Id).ToArray());
        }
        catch (ApiFailure e) { return Failure(e); }
    }
    public Task<BorrowingOperationResult> ReturnAsync(int studentId, int equipmentId, DateOnly actualReturnDate, CancellationToken cancellationToken = default) => Task.FromResult(BorrowingOperationResult.Rejected("Select the exact borrowing record to return."));
    public async Task<BorrowingOperationResult> ReturnByIdAsync(int borrowingId, DateOnly actualReturnDate, CancellationToken cancellationToken = default)
    {
        try { var dto = await SendAsync<BorrowingDto>(HttpMethod.Post, $"borrowings/{borrowingId}/return", new { }, cancellationToken); return BorrowingOperationResult.Success("Equipment returned successfully.", new[] { dto!.Id }); }
        catch (ApiFailure e) { return Failure(e); }
    }
}
