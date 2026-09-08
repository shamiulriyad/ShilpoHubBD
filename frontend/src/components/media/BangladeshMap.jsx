const districtCoordinates = {
  Barguna: [22.0953, 90.1121], Barishal: [22.7010, 90.3535], Bhola: [22.6859, 90.6482],
  Jhalokati: [22.6425, 90.1987], Patuakhali: [22.3596, 90.3299], Pirojpur: [22.5791, 89.9750],
  Bandarban: [22.1953, 92.2184], Brahmanbaria: [23.9571, 91.1119], Chandpur: [23.2333, 90.6710],
  Chattogram: [22.3569, 91.7832], Cumilla: [23.4607, 91.1809], "Cox's Bazar": [21.4272, 92.0058],
  Feni: [22.9409, 91.4067], Khagrachhari: [23.1193, 91.9847], Lakshmipur: [22.9447, 90.8282],
  Noakhali: [22.8246, 91.1017], Rangamati: [22.7324, 92.2985], Dhaka: [23.8103, 90.4125],
  Faridpur: [23.6071, 89.8429], Gazipur: [24.0023, 90.4264], Gopalganj: [23.0051, 89.8266],
  Kishoreganj: [24.4449, 90.7766], Madaripur: [23.1641, 90.1897], Manikganj: [23.8644, 90.0047],
  Munshiganj: [23.5422, 90.5305], Narayanganj: [23.6238, 90.5000], Narsingdi: [23.9220, 90.7177],
  Rajbari: [23.7574, 89.6445], Shariatpur: [23.2423, 90.4348], Tangail: [24.2513, 89.9167],
  Bagerhat: [22.6516, 89.7859], Chuadanga: [23.6402, 88.8418], Jashore: [23.1634, 89.2182],
  Jhenaidah: [23.5448, 89.1539], Khulna: [22.8456, 89.5403], Kushtia: [23.9013, 89.1205],
  Magura: [23.4873, 89.4198], Meherpur: [23.7622, 88.6318], Narail: [23.1725, 89.5127],
  Satkhira: [22.7185, 89.0705], Jamalpur: [24.9375, 89.9378], Mymensingh: [24.7471, 90.4203],
  Netrokona: [24.8835, 90.7284], Sherpur: [25.0205, 90.0153], Bogura: [24.8465, 89.3776],
  জয়পুরহাট: [25.0968, 89.0227], Joypurhat: [25.0968, 89.0227], Naogaon: [24.7936, 88.9318],
  Natore: [24.4102, 89.0076], Chapainawabganj: [24.5965, 88.2775], Pabna: [24.0064, 89.2372],
  Rajshahi: [24.3745, 88.6042], Sirajganj: [24.4534, 89.7007], Dinajpur: [25.6217, 88.6354],
  Gaibandha: [25.3288, 89.5281], Kurigram: [25.8054, 89.6362], Lalmonirhat: [25.9923, 89.2847],
  Nilphamari: [25.9318, 88.8560], Panchagarh: [26.3331, 88.5578], Rangpur: [25.7439, 89.2752],
  Thakurgaon: [26.0337, 88.4617], Habiganj: [24.3749, 91.4155], Moulvibazar: [24.4829, 91.7774],
  Sunamganj: [25.0658, 91.3950], Sylhet: [24.8949, 91.8687],
};

function buildMapUrl(coordinates) {
  const [latitude, longitude] = coordinates;
  const span = 0.34;
  const bbox = [longitude - span, latitude - span, longitude + span, latitude + span].join('%2C');
  return `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${latitude}%2C${longitude}`;
}

export default function BangladeshMap({ selectedDistrict }) {
  const coordinates = districtCoordinates[selectedDistrict] || [23.6850, 90.3563];
  const mapUrl = buildMapUrl(coordinates);
  const mapPageUrl = `https://www.openstreetmap.org/#map=${selectedDistrict ? 11 : 7}/${coordinates[0]}/${coordinates[1]}`;

  return (
    <div className="relative aspect-[16/10] overflow-hidden rounded-3xl border border-title/10 bg-title shadow-[0_18px_38px_rgba(23,59,53,0.16)]">
      <iframe
        title="Interactive map of Bangladesh"
        key={selectedDistrict || 'bangladesh'}
        src={mapUrl}
        loading="lazy"
        className="absolute inset-0 h-full w-full border-0"
        referrerPolicy="no-referrer"
      />
      <div className="pointer-events-none absolute inset-x-0 top-0 flex items-start justify-between bg-gradient-to-b from-title/55 to-transparent p-4">
        <span className="rounded-full bg-title/80 px-3 py-1.5 text-[11px] font-semibold text-surface shadow-sm backdrop-blur">
          {selectedDistrict ? `${selectedDistrict} selected` : 'Bangladesh heritage map'}
        </span>
        <span className="rounded-full bg-surface/90 px-3 py-1.5 text-[10px] font-bold uppercase tracking-[0.12em] text-title shadow-sm">
          Drag · zoom · explore
        </span>
      </div>
      <a
        href={mapPageUrl}
        target="_blank"
        rel="noreferrer"
        className="absolute bottom-3 right-3 rounded-full bg-surface/95 px-3 py-1.5 text-[11px] font-semibold text-title shadow-sm transition hover:bg-primary hover:text-surface"
      >
        Open full map ↗
      </a>
    </div>
  );
}