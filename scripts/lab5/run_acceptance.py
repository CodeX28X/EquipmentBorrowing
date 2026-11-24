"""Lab 5 T01–T06 diagnostic runner. Python standard library only."""
import datetime as dt, json, os, pathlib, socket, subprocess, time, urllib.request, urllib.error
ROOT=pathlib.Path(__file__).resolve().parents[2]
SQL=ROOT/'scripts/lab5'
OUT=ROOT/'docs/lab5/evidence'/dt.datetime.now().strftime('%Y%m%d-%H%M%S')
OUT.mkdir(parents=True,exist_ok=False)
BASE='http://127.0.0.1:5081'
DB='equipment_borrowing_lab5_acceptance'
P=None; LOG=None; TEST='setup'; SEQ=0; results=[]

def record(kind,data):
    global SEQ
    SEQ+=1
    with (OUT/'events.jsonl').open('a',encoding='utf-8') as f:
        f.write(json.dumps(dict(test=TEST,step=SEQ,time=dt.datetime.now().astimezone().isoformat(),kind=kind,data=data),ensure_ascii=False)+'\n')

def check(ok,msg):
    record('assertion',{'pass':bool(ok),'message':msg})
    if not ok: raise AssertionError(msg)

def pg(*args):
    r=subprocess.run([os.environ['LAB5_PSQL'],'-X','-w','-v','ON_ERROR_STOP=1',*args],capture_output=True,text=True,encoding='utf-8',errors='replace',env={**os.environ,'PGCLIENTENCODING':'UTF8'})
    if r.returncode: raise RuntimeError('psql failed; inspect database configuration locally (credentials not logged)')
    return r.stdout.strip()

def snap(label):
    q='''select json_build_object('database',current_database(),'date',(now() at time zone 'Asia/Manila')::date,'students',(select coalesce(json_agg(s order by "StudentId"),'[]') from "Students" s),'equipment',(select coalesce(json_agg(e order by "EquipmentId"),'[]') from "Equipment" e),'borrowings',(select coalesce(json_agg(b order by "BorrowingId"),'[]') from "Borrowings" b));'''
    d=json.loads(pg('-At','-c',q)); check(d['database']==DB,'Independent database identity'); record('postgresql',{'label':label,'state':d}); return d

def sql(name):
    check(pg('-At','-c','select current_database()')==DB,'SQL target is disposable'); record('sql',{'file':name,'output':pg('-f',str(SQL/name))})

def request(method,path,status=200,body=None,root=False):
    url=BASE+('' if root else '/api/v1')+path
    data=None if body is None else json.dumps(body).encode()
    req=urllib.request.Request(url,data=data,method=method,headers={'Content-Type':'application/json'} if data is not None else {})
    try:
        with urllib.request.urlopen(req,timeout=20) as r: code=r.status; raw=r.read().decode(); headers=dict(r.headers)
    except urllib.error.HTTPError as e: code=e.code; raw=e.read().decode(); headers=dict(e.headers)
    except (OSError,urllib.error.URLError) as e:
        record('transport',{'method':method,'path':path,'error':type(e).__name__}); raise
    try: payload=json.loads(raw)
    except ValueError: payload=raw
    record('http',{'method':method,'url':url,'request':body,'status':code,'location':next((v for k,v in headers.items() if k.lower()=='location'),None),'response':payload})
    check(code==status,f'{method} {path}: expected {status}, received {code}')
    if code>=400: check(isinstance(payload,dict) and isinstance(payload.get('code'),str) and isinstance(payload.get('message'),str),'Error contract')
    return payload,headers

def get(path,status=200): return request('GET',path,status)[0]
def rows(d,key): return d[key]
def row(d,key,idkey,id): return next(x for x in d[key] if x[idkey]==id)
def same(a,b): return all(a[k]==b[k] for k in ('students','equipment','borrowings'))
def mutate(method,path,status,body):
    before=snap('before '+method+' '+path)
    try: result,headers=request(method,path,status,body)
    finally: after=snap('after '+method+' '+path)
    if status>=400: check(same(before,after),'Rejected/failed mutation leaves all rows unchanged')
    return result,headers,before,after

def start():
    global P,LOG
    with socket.socket() as s:
        check(s.connect_ex(('127.0.0.1',5081))!=0,'Dedicated API port is free')
    LOG=open(OUT/f'api-{SEQ}.log','w',encoding='utf-8')
    P=subprocess.Popen(['dotnet','run','--no-build','--project','src/EquipmentBorrowing.Api','--no-launch-profile','--','--urls',BASE,'--environment','Development'],cwd=ROOT,stdout=LOG,stderr=subprocess.STDOUT)
    for _ in range(60):
        if P.poll() is not None: raise RuntimeError('API exited; inspect local API log')
        try:
            with urllib.request.urlopen(BASE+'/openapi/v1.json',timeout=1): return
        except (OSError,urllib.error.URLError): time.sleep(1)
    raise RuntimeError('API failed to become ready')

def stop():
    global P,LOG
    if P is not None:
        if P.poll() is None:
            if os.name=='nt': subprocess.run(['taskkill','/PID',str(P.pid),'/T','/F'],capture_output=True)
            else: P.terminate()
        P.wait(timeout=15); P=None
    if LOG: LOG.close(); LOG=None

def restart():
    before=snap('before API restart'); stop(); start(); get('/students'); after=snap('after API restart'); check(same(before,after),'Persisted rows survive API restart')

def reset():
    global S,E,B,DUE,TODAY
    sql('reset.sql'); d=snap('reset fixture')
    check((len(d['students']),len(d['equipment']),len(d['borrowings']))==(4,7,4),'Fixture counts')
    S={x['studentNumber']:x['id'] for x in get('/students')}; E={x['equipmentCode']:x['id'] for x in get('/equipment')}; B=get('/borrowings')
    check(set(S)=={'ST-100','ST-200','ST-300','ST-400'} and set(E)=={f'EQ-{i:03}' for i in range(1,8)},'API points to controlled fixture')
    TODAY=d['date']; DUE=(dt.date.fromisoformat(TODAY)+dt.timedelta(days=2)).isoformat()

def sid(n): return S[f'ST-{n}00']
def eid(n): return E[f'EQ-{n:03}']
def loan(s,e): return next(x['id'] for x in B if x['studentId']==sid(s) and x['equipmentId']==eid(e))
def issue(s,e,status=201,due=None): return mutate('POST','/borrowings',status,{'studentId':sid(s),'equipmentId':eid(e),'expectedReturnDate':due or DUE})
def multi(s,es,status=201,due=None): return mutate('POST','/checkout-requests',status,{'studentId':sid(s),'equipmentIds':[eid(e) for e in es],'expectedReturnDate':due or DUE})
def ret(id,status=200): return mutate('POST',f'/borrowings/{id}/return',status,{})
def active(d,s): return sum(x['StudentId']==sid(s) and x['Status']==0 for x in d['borrowings'])
def issued(result,before,after,s,es):
    check(len(after['borrowings'])==len(before['borrowings'])+len(es),'Exact borrowing row increase')
    check(active(after,s)==active(before,s)+len(es),'Correct active loan count')
    for e in es: check(not row(after,'equipment','EquipmentId',eid(e))['IsAvailable'],'Issued equipment unavailable')

def t01():
    before=snap('read baseline'); check(len(get('/equipment'))==7,'All equipment'); check({x['id'] for x in get('/equipment?available=true')}=={eid(n) for n in (2,3,6,7)},'Availability filter')
    check(len(get('/students'))==4,'Student list'); check(get(f'/students/{sid(1)}')['studentNumber']=='ST-100','Student detail'); check(get(f'/equipment/{eid(1)}')['equipmentCode']=='EQ-001','Equipment detail'); check(get(f'/borrowings/{loan(1,1)}')['status']=='Active','Borrowing detail')
    check(len(get(f'/students/{sid(1)}/borrowings'))==2,'S1 history'); check(len(get(f'/equipment/{eid(1)}/borrowings'))==1,'E1 history'); check(len(get('/borrowings?status=Active'))==3,'Active list'); check(get(f'/students/{sid(4)}/borrowings')==[],'Empty history')
    for p in ('/students/2147483647','/equipment/2147483647','/borrowings/2147483647','/not-a-route'): get(p,404)
    request('DELETE','/equipment',405); spec=request('GET','/openapi/v1.json',root=True)[0]; (OUT/'openapi.json').write_text(json.dumps(spec,indent=2),encoding='utf-8')
    expected={'/equipment':{'get':[200,400,500],'post':[201,400,409,500]},'/students':{'get':[200,400,500],'post':[201,400,409,500]},'/students/{id}':{'get':[200,404,500],'put':[200,400,404,409,500]},'/students/{id}/eligibility':{'patch':[200,400,404,500]},'/equipment/{id}':{'get':[200,404,500],'put':[200,400,404,409,500]},'/borrowings':{'get':[200,400,500],'post':[201,400,404,409,500]},'/checkout-requests':{'post':[201,400,404,409,500]},'/borrowings/{id}/return':{'post':[200,400,404,409,500]},'/borrowings/{id}':{'get':[200,404,500]},'/students/{id}/borrowings':{'get':[200,404,500]},'/equipment/{id}/borrowings':{'get':[200,404,500]}}
    check(sum(len(x) for x in expected.values())==16,'Expected 16 operation inventory')
    for path,methods in expected.items():
        for method,codes in methods.items():
            op=spec['paths']['/api/v1'+path][method]; check(all(str(c) in op['responses'] for c in codes),f'OpenAPI {method} {path} status codes')
            for c in codes: check(any('schema' in media for media in op['responses'][str(c)].get('content',{}).values()),f'OpenAPI {method} {path} response {c} schema')
            if method!='get': check(bool(op.get('requestBody',{}).get('content')),'OpenAPI request schema present')
    check(same(before,snap('read after')),'Read-only operations preserve database'); record('limitation','Full schema field/type/required/parameter semantic comparison remains manual.')

def location(headers,expected):
    loc=next((v for k,v in headers.items() if k.lower()=='location'),''); check(loc.endswith('/api/v1'+expected),'Location points to resource'); return get(expected)

def t02():
    body={'studentNumber':'ST-500','fullName':'Acceptance Student','contact':None}
    x,h,b,a=mutate('POST','/students',201,body); i=x['id']; check(len(a['students'])==len(b['students'])+1 and x['isEligible'] and x['contact'] is None,'New eligible student'); location(h,f'/students/{i}')
    x,h,b,a=mutate('PUT',f'/students/{i}',200,{**body,'fullName':'Edited Student','contact':'test contact'}); check(row(a,'students','StudentId',i)['StudentName']=='Edited Student' and row(a,'students','StudentId',i)['ContactInformation']=='test contact','Student edit persisted')
    mutate('POST','/students',400,{**body,'fullName':' '}); mutate('POST','/students',409,{**body,'studentNumber':' st-100 '})
    _,_,b,a=mutate('PATCH',f'/students/{sid(1)}/eligibility',200,{'isEligible':False}); check(b['borrowings']==a['borrowings'] and not row(a,'students','StudentId',sid(1))['IsAllowedToBorrow'],'Restriction preserves history')
    issue(1,2,409); ret(loan(1,1)); mutate('PATCH',f'/students/{sid(1)}/eligibility',200,{'isEligible':True}); x,h,b,a=issue(1,2); issued(x,b,a,1,[2]); _,_,b,a=mutate('PATCH',f'/students/{sid(1)}/eligibility',200,{'isEligible':True}); check(same(b,a),'Repeated eligibility is no-op'); restart()

def t03():
    body={'equipmentCode':'EQ-008','name':'Acceptance Item','description':None}; x,h,b,a=mutate('POST','/equipment',201,body); i=x['id']; check(len(a['equipment'])==len(b['equipment'])+1 and x['isAvailable'],'New available item'); location(h,f'/equipment/{i}')
    _,_,b,a=mutate('PUT',f'/equipment/{i}',200,{**body,'name':'Edited Item'}); check(row(a,'equipment','EquipmentId',i)['EquipmentName']=='Edited Item','Item edit persisted')
    _,_,b,a=mutate('PUT',f'/equipment/{eid(1)}',200,{'equipmentCode':'EQ-001','name':'Camera','description':'Edited'}); check(b['borrowings']==a['borrowings'] and not row(a,'equipment','EquipmentId',eid(1))['IsAvailable'],'Unavailable state and history preserved')
    mutate('POST','/equipment',400,{**body,'name':' '}); mutate('POST','/equipment',409,{**body,'equipmentCode':' eq-001 '}); mutate('PUT',f'/equipment/{eid(1)}',400,{'equipmentCode':'EQ-001','name':'Camera','description':None,'isAvailable':True}); restart()

def t04():
    x,h,b,a=issue(4,2); issued(x,b,a,4,[2]); i=x['id']; location(h,f'/borrowings/{i}'); check(get(f'/borrowings/{i}')['status']=='Active','Created borrowing active')
    issue(2,6,409); issue(4,1,409); mutate('POST','/borrowings',404,{'studentId':2147483647,'equipmentId':eid(6),'expectedReturnDate':DUE}); issue(4,6,400,TODAY)
    x,h,b,a=ret(i); check(len(a['borrowings'])==len(b['borrowings']) and row(a,'borrowings','BorrowingId',i)['Status']==1 and row(a,'equipment','EquipmentId',eid(2))['IsAvailable'],'Same borrowing returned and equipment available'); check(get(f'/borrowings/{i}')['returnedDate']==TODAY,'Server return date'); ret(i,409); restart()

def t05():
    x,h,b,a=multi(4,[2,6]); check(len(x['borrowings'])==2,'Two response records'); issued(x,b,a,4,[2,6]); reset()
    x,h,b,a=issue(3,2); issued(x,b,a,3,[2]); check(active(a,3)==3,'Single checkout reaches limit'); reset()
    x,h,b,a=multi(1,[2,6]); issued(x,b,a,1,[2,6]); check(active(a,1)==3,'Multi checkout reaches limit'); reset()
    multi(3,[2,6],409); reset()
    multi(4,[2,2],400); multi(4,[2],400); multi(4,[1,2],409); multi(2,[2,6],409); mutate('POST','/checkout-requests',404,{'studentId':sid(4),'equipmentIds':[eid(2),2147483647],'expectedReturnDate':DUE}); multi(4,[2,6],400,TODAY); reset()
    sql('fault-on.sql')
    try: multi(4,[2,6],500)
    finally: sql('fault-off.sql')

def t06():
    before=snap('read baseline')
    for s,n in [(1,2),(4,0)]: check(len(get(f'/students/{sid(s)}/borrowings'))==n,'Student history count')
    for e,n in [(1,1),(3,1)]: check(len(get(f'/equipment/{eid(e)}/borrowings'))==n,'Equipment history count')
    get('/students/2147483647/borrowings',404); get('/equipment/2147483647/borrowings',404)
    for q,ids in [('aDa',{loan(1,1),loan(1,3)}),('ST-300',{loan(3,4),loan(3,5)}),('pRoJeC',{loan(1,3)})]: check({x['id'] for x in get('/borrowings?search='+q)}==ids,'Search '+q)
    for status,count in [('All',4),('Active',3),('Returned',1)]:
        r=get('/borrowings?status='+status); check(len(r)==count and all(status=='All' or x['status']==status for x in r),'Status '+status)
    for field in ('borrowedDate','expectedReturnDate'):
        for direction in ('asc','desc'):
            r=get(f'/borrowings?sort={field}&direction={direction}'); expected=sorted(sorted(r,key=lambda x:x['id']),key=lambda x:x[field],reverse=direction=='desc'); check(r==expected and len(r)==4,'Stable sort '+field+' '+direction)
    check([x['id'] for x in get('/borrowings?search=ada&status=Returned&sort=expectedReturnDate&direction=desc')]==[loan(1,3)],'Combined criteria'); check(len(get('/borrowings'))==4,'Clear criteria'); check(get('/borrowings?search=not-found-xyz')==[],'No matches'); get('/borrowings?status=invalid',400); check(same(before,snap('read after')),'Read-only state unchanged')

try:
    if os.environ.get('PGDATABASE')!=DB or os.environ.get('PGHOST') not in ('localhost','127.0.0.1','::1'): raise RuntimeError('Refusing nonlocal/non-disposable target')
    check(pg('-At','-c','select current_database()')==DB,'Database identity before starting API'); start()
    for f in (t01,t02,t03,t04,t05,t06):
        TEST=f.__name__.upper()
        try: reset(); f(); results.append({'test':TEST,'result':'automated checks passed','limitation':'Full OpenAPI semantic audit pending' if TEST=='T01' else ''})
        except Exception as e:
            results.append({'test':TEST,'result':'failed or incomplete','reason':str(e) if isinstance(e,AssertionError) else type(e).__name__}); print(TEST,results[-1]); break
        print(TEST,results[-1]['result'],flush=True)
finally:
    stop()
    (OUT/'summary.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
    print('Evidence folder:',OUT)
if len(results)!=6 or any(x['result']!='automated checks passed' for x in results): raise SystemExit(1)
