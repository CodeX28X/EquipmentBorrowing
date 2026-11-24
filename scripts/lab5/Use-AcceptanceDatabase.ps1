$lab5Connection = [Environment]::GetEnvironmentVariable(
    'EQUIPMENTBORROWING_CONNECTION_STRING',
    'User'
)

if ([string]::IsNullOrWhiteSpace($lab5Connection)) {
    throw 'Development connection string is missing.'
}

$lab5Builder = [System.Data.Common.DbConnectionStringBuilder]::new()

$lab5Builder.set_ConnectionString($lab5Connection)

$lab5Host = $null

foreach ($key in @('Host', 'Server', 'Data Source')) {
    if ($lab5Builder.ContainsKey($key)) {
        $lab5Host = [string]$lab5Builder.get_Item($key)
        break
    }
}

if ($lab5Host -notin @('localhost', '127.0.0.1', '::1')) {
    throw 'Acceptance database must be local.'
}

foreach ($key in @('Database', 'Initial Catalog')) {
    if ($lab5Builder.ContainsKey($key)) {
        $lab5Builder.Remove($key) | Out-Null
    }
}

$lab5Builder.set_Item(
    'Database',
    'equipment_borrowing_lab5_acceptance'
)

$env:EQUIPMENTBORROWING_CONNECTION_STRING =
    $lab5Builder.get_ConnectionString()

Write-Host 'API terminal now targets local equipment_borrowing_lab5_acceptance (process only).'
