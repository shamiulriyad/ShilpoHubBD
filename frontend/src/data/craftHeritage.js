// Editorial reference data is separate from marketplace records and seller certification.
export const giSource = 'https://dpdt.gov.bd/pages/static-pages/6922dff0933eb65569e24bcf';
const handicrafts = ['Banglapedia · Handicrafts', 'https://en.banglapedia.org/index.php?title=Handicrafts'];
const folk = ['Banglapedia · Folk art and crafts', 'https://en.banglapedia.org/index.php?title=Folk_Art_and_Crafts'];
const gi = ['DPDT · Registered geographical indications', giSource];
export const craftHeritage = [
  {
    slug: 'jamdani-weaving', name: 'Dhakai Jamdani', aliases: ['Dhakaiya Jamdani', 'Jamdani Weaving'],
    region: 'Dhaka division · Narayanganj', type: 'Textile', giName: 'Jamdani Saree (জামদানি শাড়ী)',
    unesco: 'Traditional art of Jamdani weaving · 2013',
    summary: 'An exceptionally fine woven textile whose motifs are formed on the loom, one small detail at a time.',
    history: 'Jamdani belongs to the fine cotton weaving traditions of the Dhaka region. Skills pass between master weavers and apprentices.',
    materials: 'Cotton yarn; materials vary by contemporary maker.',
    process: 'Weavers insert supplementary threads into the cloth to build floral and geometric motifs while weaving.',
    products: 'Saris, scarves and decorative textiles.',
    story: 'The pattern grows with the weaver’s hands. Learning to see that patient work is part of appreciating a Jamdani.',
    visit: 'Explore Narayanganj weaving heritage. Arrange a workshop visit with a host before travelling.',
    sources: [gi, ['UNESCO · Jamdani weaving', 'https://ich.unesco.org/en/RL/traditional-art-of-jamdani-weaving-00879']],
  },
  {
    slug: 'dhakai-muslin', name: 'Dhakai Muslin', aliases: ['Dhakai Moslin', 'ঢাকাই মসলিন'],
    region: 'Dhaka division · Dhaka region', type: 'Textile', giName: 'Dhakai Muslin (ঢাকাই মসলিন)',
    summary: 'The celebrated fine cotton cloth associated with Dhaka, known for its lightness and delicate texture.',
    history: 'Dhaka’s muslin became renowned through historic textile trade and court patronage. Its history connects cotton cultivation, skilled spinning and weaving.',
    materials: 'Fine cotton yarn; historic muslin depended on specialist cotton and spinning knowledge.',
    process: 'Cotton is prepared, spun into fine yarn and woven into lightweight fabric.',
    products: 'Fine cloth, saris and scarves.',
    story: 'A seemingly simple length of cloth can hold generations of knowledge about fibre, touch and patience.',
    visit: 'Ask museums and weaving hosts about muslin history and contemporary production. Confirm the origin and composition of any textile you buy.',
    sources: [gi, ['Banglapedia · Muslin', 'https://en.banglapedia.org/index.php/Muslin']],
  },
  {
    slug: 'rajshahi-silk', name: 'Rajshahi Silk', aliases: ['রাজশাহী সিল্ক'],
    region: 'Rajshahi division · Rajshahi', type: 'Textile', giName: 'Rajshahi Silk (রাজশাহী সিল্ক)',
    summary: 'A signature textile of Rajshahi, valued for its lustre, soft drape and connection to the region’s silk industry.',
    history: 'Silk production is an important part of Rajshahi’s regional identity, connecting sericulture with yarn preparation and textile making.',
    materials: 'Silk yarn; check the maker’s composition details for blends.',
    process: 'Silk yarn is prepared, woven and finished; dyeing and decorative techniques vary by workshop.',
    products: 'Saris, scarves, dress fabrics and accessories.',
    story: 'Behind the sheen is a chain of skilled work, from the people who prepare the yarn to those who weave the finished fabric.',
    visit: 'Explore Rajshahi’s silk shops and arrange factory or workshop visits directly. Ask about yarn origin and care.',
    sources: [gi, ['Rajshahi district · Silk heritage', 'https://file-rajshahi.portal.gov.bd/files/rajshahi.gov.bd/files/042472c0_aa62_4ff6_b1b5_76bc9b80b4f3/5b5130d2473327dbfe6593f864fbe696.pdf']],
  },
  {
    slug: 'shital-pati', name: 'Shital Pati', region: 'Sylhet division · Greater Sylhet', type: 'Fibre craft',
    giName: 'Bangladesh Shital Pati (বাংলাদেশের শীতল পাটি)', unesco: 'Traditional art of Shital Pati weaving of Sylhet · 2017',
    summary: 'Cool, smooth mats woven from carefully prepared murta strips.',
    materials: 'Murta plant strips.', process: 'Stems are split and prepared before strips are interlaced into mats.',
    products: 'Sleeping and sitting mats.',
    history: 'A household craft sustained by weaving communities, especially in low-lying villages of greater Sylhet.',
    story: 'A mat is both an everyday comfort and a record of the maker’s skill.',
    sources: [gi, ['UNESCO · Shital Pati weaving', 'https://ich.unesco.org/en/RL/traditional-art-of-shital-pati-weaving-of-sylhet-01112']],
  },
  {
    slug: 'rickshaw-art', name: 'Rickshaw Art', region: 'Dhaka', type: 'Visual art',
    unesco: 'Rickshaws and rickshaw painting in Dhaka · 2023',
    summary: 'Colourful painted vehicles bring popular imagery and storytelling into everyday city life.',
    materials: 'Paint and decorated vehicle panels.', process: 'Artists paint and decorate rickshaw surfaces.',
    products: 'Painted rickshaws and decorative panels.',
    history: 'An urban folk art associated with Dhaka’s rickshaw workshops.',
    story: 'The streets become a moving gallery, carrying artists’ work into daily journeys.',
    sources: [['UNESCO · Rickshaws and rickshaw painting', 'https://ich.unesco.org/en/RL/rickshaws-and-rickshaw-painting-in-dhaka-01589']],
  },
  ...[
    ['nakshi-kantha', 'Nakshi Kantha', 'Textile', 'Embroidered quilts turn layered cloth into patterned household textiles.', 'Cloth and thread', 'Layering and hand stitching', 'Quilts and embroidered textiles', folk],
    ['pottery-terracotta', 'Pottery & Terracotta', 'Clay craft', 'Shaped and fired clay serves everyday, decorative and architectural purposes.', 'Clay', 'Shaping, drying and firing', 'Pots, vessels and relief panels', folk],
    ['jute-craft', 'Jute Craft', 'Fibre craft', 'Natural jute fibres are worked into practical household objects.', 'Jute fibre and yarn', 'Twisting, weaving and stitching', 'Bags, baskets and homeware', handicrafts],
    ['bamboo-cane', 'Bamboo & Cane', 'Fibre craft', 'Split plant materials become lightweight woven and framed objects.', 'Bamboo and cane', 'Splitting, weaving and joining', 'Baskets, furniture and containers', handicrafts],
    ['handloom-textiles', 'Handloom Textiles', 'Textile', 'A broad family of fabrics woven on hand-operated looms.', 'Yarn; fibre varies by tradition', 'Warp preparation and loom weaving', 'Saris, lungis and scarves', handicrafts],
    ['brass-bell-metal', 'Brass & Bell Metal', 'Metal craft', 'Metalworkers form durable utensils and ceremonial objects.', 'Brass and bell-metal alloys', 'Casting, hammering and finishing', 'Utensils and ceremonial objects', handicrafts],
    ['wood-carving', 'Wood Carving', 'Wood craft', 'Carved surfaces give furniture and architectural elements their character.', 'Wood', 'Cutting and carving', 'Furniture and decorative panels', folk],
    ['folk-painting-alpana', 'Folk Painting & Alpana', 'Visual art', 'Painted imagery and floor patterns express social and ritual traditions.', 'Pigments; rice paste in traditional alpana', 'Painting and pattern drawing', 'Floor designs and painted objects', folk],
    ['jewellery-metalwork', 'Jewellery & Metalwork', 'Metal craft', 'Worked metal becomes ornaments for personal and ceremonial use.', 'Metals and decorative materials', 'Forming, joining and ornamenting', 'Jewellery and ornaments', handicrafts],
    ['leather-craft', 'Leather Craft', 'Leather craft', 'Cut and stitched leather is fashioned into wearable and practical goods.', 'Leather and thread', 'Cutting, stitching and finishing', 'Footwear, bags and accessories', handicrafts],
    ['natural-dye-batik', 'Natural Dye & Batik', 'Textile', 'Colouring and resist processes create patterned cloth; batik uses wax to resist dye.', 'Fabric, dyes and wax for batik', 'Dyeing and resist patterning', 'Patterned cloth and garments', handicrafts],
    ['embroidery', 'Embroidery', 'Textile', 'Decorative stitching adds patterns and texture to cloth.', 'Fabric and thread', 'Hand stitching', 'Garments and household textiles', folk],
    ['clay-dolls', 'Clay Dolls', 'Clay craft', 'Modelled figures and toys are part of folk craft traditions.', 'Clay and pigments', 'Modelling and decorating', 'Dolls, figures and toys', folk],
  ].map(([slug, name, type, summary, materials, process, products, source]) => ({
    slug, name, type, summary, materials, process, products, region: 'Bangladesh · Multiple local traditions',
    sources: [source],
  })),
];

export function heritageForCategory(category) {
  const key = (category?.slug || '').toLowerCase();
  const name = (category?.name || '').toLowerCase();
  return craftHeritage.find(c => c.slug === key || [c.name, ...(c.aliases || [])].some(n => n.toLowerCase() === name));
}

export function filterCraftHeritage(records, query, recognition) {
  const term = query.trim().toLocaleLowerCase();
  return records.filter(c => (!term || [c.name, c.region, c.type, c.summary, c.materials, ...(c.aliases || [])].join(' ').toLocaleLowerCase().includes(term))
    && (recognition === 'all' || (recognition === 'gi' && c.giName) || (recognition === 'unesco' && c.unesco)));
}
