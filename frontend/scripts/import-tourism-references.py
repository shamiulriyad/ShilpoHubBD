"""Refresh public reference content and openly licensed photographs; never writes application DB."""
import json, re, html, urllib.request, urllib.parse, urllib.error, time
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor

ROOT = Path(__file__).resolve().parents[1]
HEADERS = {'User-Agent': 'ShilpoHubHeritage/1.0 (educational destination directory)'}
def get(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=HEADERS), timeout=40) as response:
        return response.read()
def api(host, **params):
    return json.loads(get('https://' + host + '/w/api.php?' + urllib.parse.urlencode(dict(action='query', format='json', **params))))
def plain(value):
    return html.unescape(re.sub('<[^>]+>', '', value or '')).strip()

districts = 'Barguna|Barishal|Bhola|Jhalokati|Patuakhali|Pirojpur|Bandarban|Brahmanbaria|Chandpur|Chattogram|Cumilla|Cox’s Bazar|Feni|Khagrachhari|Lakshmipur|Noakhali|Rangamati|Dhaka|Faridpur|Gazipur|Gopalganj|Kishoreganj|Madaripur|Manikganj|Munshiganj|Narayanganj|Narsingdi|Rajbari|Shariatpur|Tangail|Bagerhat|Chuadanga|Jashore|Jhenaidah|Khulna|Kushtia|Magura|Meherpur|Narail|Satkhira|Jamalpur|Mymensingh|Netrokona|Sherpur|Bogura|Joypurhat|Naogaon|Natore|Chapainawabganj|Pabna|Rajshahi|Sirajganj|Dinajpur|Gaibandha|Kurigram|Lalmonirhat|Nilphamari|Panchagarh|Rangpur|Thakurgaon|Habiganj|Moulvibazar|Sunamganj|Sylhet'.split('|')
aliases = {'Barishal':'Barisal','Chattogram':'Chittagong','Cumilla':'Comilla','Cox’s Bazar':"Cox's Bazar",'Jashore':'Jessore','Bogura':'Bogra','Chapainawabganj':'Chapai Nawabganj','Moulvibazar':'Moulvibazar'}
destinations = [
 ('haringhata','Haringhata forest','Barguna','Coastal forest','Mangrove walks and estuary landscapes'),
 ('panam','Panam Nagar','Narayanganj','Built heritage','Historic merchant houses and Sonargaon heritage'),
 ('sixty-dome','Sixty Dome Mosque','Bagerhat','Built heritage','Sultanate architecture and the historic mosque city'),
 ('paharpur','Somapura Mahavihara','Naogaon','Archaeology','Buddhist monastic heritage at Paharpur'),
 ('mahasthan','Mahasthangarh','Bogura','Archaeology','Ancient Pundranagara and archaeological remains'),
 ('lalbagh','Lalbagh Fort','Dhaka','Built heritage','Mughal architecture in Old Dhaka'),
 ('puthia','Puthia Temple Complex','Rajshahi','Built heritage','Temple architecture and terracotta ornament'),
 ('kantajew','Kantajew Temple','Dinajpur','Built heritage','Terracotta temple art'),
 ('tajhat','Tajhat Palace','Rangpur','Built heritage','Palace architecture and museum collections'),
 ('kuakata','Kuakata','Patuakhali','Coast','Seaside landscapes and coastal communities'),
 ('ratargul','Ratargul Swamp Forest','Sylhet','Nature','Freshwater swamp forest'),
 ('lawachara','Lawachara National Park','Moulvibazar','Nature','Forest trails and biodiversity'),
 ('tanguar','Tanguar Haor','Sunamganj','Wetland','Haor landscapes and wetland livelihoods'),
 ('baliati','Baliati Zamindari','Manikganj','Built heritage','Village palaces and architectural heritage'),
 ('shilaidaha','Shilaidaha','Kushtia','Literary heritage','Tagore’s Kuthibari and riverside heritage'),
 ('mainamati','Mainamati','Cumilla','Archaeology','Buddhist archaeological remains'),
 ('coxsbazar',"Cox's Bazar Beach",'Cox’s Bazar','Coast','Sea beach and coastal scenery'),
 ('kaptai','Kaptai Lake','Rangamati','Lake','Lake landscapes and hill communities'),
 ('dhamrai','Dhamrai Upazila','Dhaka','Craft community','Metal craft and community heritage'),
 ('sonargaon','Sonargaon','Narayanganj','Craft community','Folk art, weaving and historic settlements'),
]
titles = [aliases.get(d,d) + ' District' for d in districts] + [p[1] for p in destinations]
pages = {}
for offset in range(0,len(titles),10):
    batch = titles[offset:offset+10]
    result = api('en.wikipedia.org',titles='|'.join(batch), redirects=1, prop='pageimages|coordinates|extracts', piprop='thumbnail|name', pithumbsize=800, exintro=1, explaintext=1, exlimit=20, colimit='max')
    indexed = {p['title']:p for p in result.get('query',{}).get('pages',{}).values()}
    redirects = {p['from']:p['to'] for p in result.get('query',{}).get('redirects',[])}
    redirects.update({p['from']:p['to'] for p in result.get('query',{}).get('normalized',[])})
    for title in batch:
        pages[title] = indexed.get(redirects.get(title,title), {})

def brief(page):
    sentences = re.split(r'(?<=[.!?])\s+', page.get('extract','').replace('\n',' '))
    return ' '.join(sentences[:2])
def source(page):
    return 'https://en.wikipedia.org/wiki/' + urllib.parse.quote(page.get('title','').replace(' ','_'))
assets = ROOT/'public/images/destinations'
assets.mkdir(parents=True,exist_ok=True)
previous_file = ROOT/'src/data/tourismReferences.json'
previous_data = json.loads(previous_file.read_text(encoding='utf-8')) if previous_file.exists() else {}
previous = {p['id']:p.get('image') for p in previous_data.get('places',[])}
def photo(record):
    if previous.get(record[0]): return previous[record[0]]
    page = pages.get(record[1],{})
    filename = page.get('pageimage')
    thumbnail = page.get('thumbnail',{}).get('source')
    if not filename or not thumbnail or filename.lower().endswith('.svg'): return None
    try:
        time.sleep(1)
        info = api('commons.wikimedia.org',titles='File:'+filename,prop='imageinfo',iiprop='extmetadata|url',iiurlwidth=800)
        item = next(iter(info['query']['pages'].values())).get('imageinfo',[{}])[0]
        meta = item.get('extmetadata',{})
        license_name = plain(meta.get('LicenseShortName',{}).get('value'))
        if not any(k in license_name.lower() for k in ['cc by','cc0','public domain']): return None
        extension = '.png' if filename.lower().endswith('.png') else '.jpg'
        local = record[0]+extension
        (assets/local).write_bytes(get(item.get('thumburl') or thumbnail))
        return {'url':'/images/destinations/'+local,'alt':page['title'],'author':plain(meta.get('Artist',{}).get('value')),'license':license_name,'licenseUrl':plain(meta.get('LicenseUrl',{}).get('value')),'source':item.get('descriptionurl'),'note':'Resized from the original; displayed with a cropped frame.'}
    except Exception as error:
        print('Photo unavailable:',record[0],type(error).__name__,getattr(error,'code',''))
        return None
photos = list(map(photo,destinations))
places=[]
for record,image in zip(destinations,photos):
    key,title,district,kind,known = record
    page=pages.get(title,{})
    coords=next(iter(page.get('coordinates',[])),{})
    old=next((p for p in previous_data.get('places',[]) if p['id']==key),{})
    places.append(dict(id=key,name=page.get('title',title),districtName=district,placeType=kind,knownFor=known,description=brief(page),latitude=coords.get('lat',old.get('latitude')),longitude=coords.get('lon',old.get('longitude')),coordinateSource=old.get('coordinateSource'),source=source(page),image=image,reference=True))
districtRecords=[]
for name in districts:
    page=pages.get(aliases.get(name,name)+' District',{})
    related=[p for p in places if p['districtName']==name]
    old=next((d for d in previous_data.get('districts',[]) if d['name']==name),{})
    districtRecords.append(dict(name=name,description=brief(page),source=source(page),knownFor=old.get('knownFor') or ' · '.join(p['knownFor'] for p in related),heritageSource=old.get('heritageSource'),image=next((p['image'] for p in related if p['image']),None)))
output={'retrieved':'2026-09-23','districts':districtRecords,'places':places}
(ROOT/'src/data/tourismReferences.json').write_text(json.dumps(output,ensure_ascii=False,indent=2),encoding='utf-8')
(assets/'CREDITS.json').write_text(json.dumps([p for p in photos if p],ensure_ascii=False,indent=2),encoding='utf-8')
print('Imported',len(districtRecords),'district references,',len(places),'places and',sum(bool(p) for p in photos),'licensed photos.')
print('Missing district references:',[d['name'] for d in districtRecords if not d['description']])
print('Places without coordinates:',[p['id'] for p in places if p['latitude'] is None])
