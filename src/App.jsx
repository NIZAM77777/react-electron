import { useEffect, useLayoutEffect, useRef, useState } from 'react';
import gsap from 'gsap';
import { Anchor, Box, ChevronLeft, Home, Settings, Users } from 'lucide-react';

const sections = [
  {
    id: 'company',
    title: 'Company Profile',
    subtitle: 'WHO WE ARE',
    description:
      'Discover our maritime heritage, our people, and our commitment to dependable shipping services.',
    image: './images/company_profile_buildings.png',
    details: ['Our Story', 'Leadership', 'Our Mission'],
  },
  {
    id: 'fleet',
    title: 'Fleet',
    subtitle: 'VESSELS & CAPABILITIES',
    description:
      'Explore a versatile fleet designed to support transportation, offshore operations, and marine logistics.',
    image: './images/fleet_ship.png',
    details: ['Landing Craft', 'Tug Boat', 'Work Boats'],
  },
  {
    id: 'facilities',
    title: 'Facilities',
    subtitle: 'BUILT FOR THE SEA',
    description:
      'Explore the facilities, workshops, and operational spaces that support our maritime activities.',
    image: './images/facilities_crane.png',
    details: ['Shipyard', 'Maintenance', 'Operations'],
  },
  {
    id: 'services',
    title: 'Services',
    subtitle: 'MARITIME SOLUTIONS',
    description:
      'Discover integrated marine support, vessel operations, and shipping solutions for demanding projects.',
    image: './images/services_gear_wrench.png',
    details: ['Marine Support', 'Logistics', 'Ship Management'],
  },
];

const services = [
  { label: 'FLEET OWNERSHIP & OPERATIONS', img: 'fleet-ownership', detailImage: './images/service-details/fleet-ownership.png' },
  { label: 'OFFSHORE EPC & LOGISTICS SUPPORT', img: 'offshore-epc', detailImage: './images/service-details/offshore-epc.png' },
  { label: 'FREIGHT FORWARDING & CUSTOMS CLEARANCE', img: 'freight-forwarding', detailImage: './images/service-details/freight-forwarding.png' },
  { label: 'PROJECT CARGO HANDLING AND SUPPORT', img: 'project-cargo', detailImage: './images/service-details/project-cargo.png' },
  { label: 'SHIPBUILDING AND REPAIRS', img: 'shipbuilding', detailImage: './images/service-details/shipbuilding.png' },
  { label: 'PORT AGENCY AND SHIP MANAGEMENT', img: 'port-agency', detailImage: './images/service-details/port-agency.png' },
  { label: 'TUG AND TOWAGE SERVICES', img: 'tug', detailImage: './images/service-details/tug.png' },
  { label: 'ONSHORE AND OFFSHORE OIL & GAS FIELD SUPPORT', img: 'oil-gas', detailImage: './images/service-details/oil-gas.png' },
];

const servicePoints = {
  'fleet-ownership': [
    'One of the largest vessel owners in the UAE.',
    'Current fleet of 116 vessels.',
    'Fleet includes Landing Crafts, Tug Boats, Crew Boats, Cargo Barges, and Passengers.',
    'We own and operate all our vessels.',
  ],
  'offshore-epc': [
    'Most of our vessels work with oil majors.',
    'We support EPC projects and contractors.',
    'Our vessels provide offshore logistics and marine support.',
  ],
  'freight-forwarding': [
    'We provide freight forwarding services for our clients.',
    'We transport cargo worldwide.',
    'We also provide customs clearance services.',
  ],
  'project-cargo': [
    'Transport of project and heavy cargo worldwide.',
    'Experienced team and engineers.',
    'Safe handling of heavy and oversized cargo.',
    'Proper planning and coordination.',
    'Reliable and efficient transportation.',
  ],
  shipbuilding: [
    'Shipbuilding and vessel repair services.',
    'Fully equipped shipyard facilities.',
    'Experienced project managers.',
    'Skilled and experienced manpower.',
    'Quality workmanship and safe operations.',
    'Timely completion and delivery of vessels.',
  ],
  'port-agency': [
    'In-house team for port agency services.',
    'Handling of all vessel agency requirements.',
    'Professional and efficient port support.',
    'Ship management services for vessels.',
    'Experienced team for smooth vessel operations.',
    'Reliable and timely service.',
  ],
  tug: [
    'Large fleet of tugs available.',
    'Towing services for various requirements.',
    'Tug assistance for vessels and marine operations.',
    'Safe and reliable towing operations.',
  ],
  'oil-gas': [
    'Vessel supply for onshore and offshore oil & gas operations',
    'Offshore field support and marine logistics',
    'Crew and personnel transportation',
    'Equipment and material transportation',
    'Supply and support vessel services',
    'Operational and logistics support for oil & gas fields',
    'Reliable marine support for field operations',
  ],
};

const facilityPoints = {
  'head-office': [
    'A WELL-STRUCTURED FUNCTIONAL DEPARTMENT EMPLOYING THE BEST TALENT.',
  ],
  'warehouse-storage': [
    'SECURE & WELL-ORGANIZED STORAGE',
    'DEDICATED CAPACITY FOR GOODS & EQUIPMENT',
    'EFFICIENT INVENTORY & MATERIAL FLOW',
  ],
  'shipyard-facility': [
    'NEW VESSEL CONSTRUCTION',
    'VESSEL REPAIRS, MAINTENANCE & DRY-DOCKING',
    'EXPERT TEAM FOR FLEET & THIRD-PARTY VESSELS',
  ],
  'marine-jetty': [
    '180 M QUAY WALL',
    'DEDICATED RO-RO FACILITY',
    'DEDICATED SLIPWAY',
    'BERTHING FACILITIES AND STORAGE YARD',
  ],
  'workshop-maintenance': [
    'TECHNICAL, MAINTENANCE & REPAIR SUPPORT',
    'SKILLED & EXPERIENCED PERSONNEL',
    'EFFICIENT, RELIABLE FLEET & CLIENT SUPPORT',
  ],
  'travelift-facility': [
    'SAFE VESSEL LIFTING, LAUNCHING & HAULING',
    'MAINTENANCE, REPAIR & DRY-DOCKING SUPPORT',
    'SKILLED OPERATORS & DEDICATED MARINE TEAM',
  ],
};

function ServicesScreen({ onSelectDetail }) {
  return (
    <div className="services-page">
      <h1 className="anim">
        OUR <span>SERVICES</span>
      </h1>
      <span className="services-rule anim" />
      <p className="services-sub anim">TOUCH A SERVICE TO EXPLORE</p>

      <div className="services-grid">
        {services.map((s) => (
          <button key={s.img} className="service-item anim" onClick={() => onSelectDetail(s)}>
            <span className="service-icon">
              <img
                src={`./images/services/${s.img}.png`}
                alt=""
                draggable="false"
                onError={(e) => {
                  // tug icon not added yet: fall back to the ship icon
                  if (!e.currentTarget.dataset.fallback) {
                    e.currentTarget.dataset.fallback = '1';
                    e.currentTarget.src = './images/services/fleet-ownership.png';
                  }
                }}
              />
            </span>
            <span className="service-label">{s.label}</span>
          </button>
        ))}
      </div>
    </div>
  );
}

const fleet = [
  {
    label: 'LANDING CRAFTS',
    detailTitle: 'LANDING CRAFTS',
    img: 'landing-craft',
    cx: 285,
    top: 205,
    w: 343,
    gap: 14,
    menuImage: './images/fleet/landing-craft-ortho-blue-white.png',
    detailImage: './images/fleet-details/landing-craft-new.jpg',
    specs: [
      { label: 'LENGTH', value: '40–100 M', kind: 'ship', icon: 'length' },
      { label: 'DWT', value: '250–6,793 DWT', kind: 'box', icon: 'dead-weight' },
    ],
  },
  {
    label: 'TUG BOATS',
    detailTitle: 'TOW TUG BOATS AND ANCHOR HANDLING TUGS',
    img: 'tug-boats',
    cx: 825,
    top: 198,
    w: 250,
    gap: 20,
    menuImage: './images/fleet/tug-boats-ortho.png',
    detailImage: './images/fleet-details/tug-boats-new.jpg',
    specs: [
      { label: 'LENGTH', value: '21.5–37 M', kind: 'ship', icon: 'length' },
      { label: 'BP', value: '15–60 T', kind: 'anchor' },
      { label: 'BHP', value: '1200–4400 BHP', kind: 'settings', icon: 'power' },
    ],
  },
  {
    label: 'BARGES',
    detailTitle: 'BARGES',
    img: 'barges',
    cx: 829,
    top: 492,
    w: 385,
    gap: 55,
    menuImage: './images/fleet/barges-ortho.png',
    detailImage: './images/fleet-details/barges-new.png',
    specs: [
      { label: 'LENGTH', value: '52–76 M', kind: 'ship', icon: 'length' },
      { label: 'DWT', value: '1,800–5000 DWT', kind: 'box', icon: 'dead-weight' },
    ],
  },
  {
    label: 'PASSENGER BOATS',
    detailTitle: 'PASSENGER BOATS',
    img: 'passenger-boats',
    cx: 540,
    top: 760,
    w: 352,
    gap: 36,
    menuImage: './images/fleet/passenger-boats-ortho.png',
    detailImage: './images/fleet-details/passenger-boats-new.jpeg',
    specs: [
      { label: 'LENGTH', value: '10–13.78 M', kind: 'ship', icon: 'length' },
      { label: 'CAPACITY', value: '10–30 PASSENGERS', kind: 'users' },
    ],
  },
  {
    label: 'CREW BOATS',
    detailTitle: 'CREW BOATS',
    img: 'crew-boats',
    cx: 276,
    top: 505,
    w: 300,
    gap: 14,
    menuImage: './images/fleet/crew-boats-ortho.png',
    detailImage: './images/fleet-details/crew-boats-new.png',
    specs: [
      { label: 'LENGTH', value: '33.5–42 M', kind: 'ship', icon: 'length' },
      { label: 'CAPACITY', value: 'UP TO 282 PASSENGERS', kind: 'users' },
      { label: 'DECK AREA', value: '80 - 110 M²', kind: 'box', icon: 'deck-area' },
    ],
  },
];

function FleetScreen({ onSelectFleet }) {
  return (
    <div className="services-page">
      <h1 className="anim">
        OUR <span>FLEET</span>
      </h1>
      <span className="services-rule anim" />
      <p className="services-sub anim">EXPLORE OUR VESSEL CATEGORIES</p>

      {fleet.map((f) => (
        <button
          key={f.img}
          className="fleet-item anim"
          onClick={() => onSelectFleet(f)}
          style={{ left: f.cx - f.w / 2, top: f.top, width: f.w }}
        >
          <img
            className="fleet-icon"
            src={f.menuImage}
            alt=""
            draggable="false"
          />
          <span className="fleet-label" style={{ marginTop: f.gap }}>
            {f.label}
          </span>
        </button>
      ))}
    </div>
  );
}

function FleetDetailScreen({ fleetItem, onReturn, onHome }) {
  const fleetReferenceImages = {
  };
  const fleetReferenceImage = fleetReferenceImages[fleetItem.img];

  if (fleetItem.detailImage.includes('/service-details/')) {
    const points = servicePoints[fleetItem.img] || [];

    return (
      <div className="fleet-detail-screen service-detail-video-screen">
        <button className="fleet-detail-nav fleet-detail-nav-back" onClick={onReturn} aria-label="Return">
          <ChevronLeft size={54} strokeWidth={1.5} />
          <span>RETURN</span>
        </button>
        <button className="fleet-detail-nav fleet-detail-nav-home" onClick={onHome} aria-label="Home">
          <Home size={48} strokeWidth={1.5} />
          <span>HOME</span>
        </button>

        <h1 className="service-detail-title">{fleetItem.label}</h1>
        <div className={`service-detail-points service-detail-points-${points.length}`}>
          {points.map((point) => (
            <div className="service-detail-point" key={point}>
              <span className="service-detail-point-bullet" aria-hidden="true" />
              <span>{point}</span>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (fleetItem.detailImage.includes('/facility-details/')) {
    const points = facilityPoints[fleetItem.img] || [];

    return (
      <div className="fleet-detail-screen facility-photo-screen">
        <div
          className="facility-photo-art"
          style={{ backgroundImage: `url(${fleetItem.detailImage})` }}
          aria-hidden="true"
        />
        <div className="facility-photo-shade" aria-hidden="true" />

        <button className="fleet-detail-nav fleet-detail-nav-back" onClick={onReturn} aria-label="Return">
          <ChevronLeft size={26} strokeWidth={1.5} />
          <span>RETURN</span>
        </button>
        <button className="fleet-detail-nav fleet-detail-nav-home" onClick={onHome} aria-label="Home">
          <Home size={26} strokeWidth={1.5} />
          <span>HOME</span>
        </button>

        <h1 className="facility-photo-title">{fleetItem.detailTitle || fleetItem.label}</h1>
        <div className={`facility-photo-points ${fleetItem.img === 'head-office' ? 'facility-photo-points-head-office' : ''}`}>
          {points.map((point) => (
            <div className="facility-photo-point" key={point}>
              <span className="facility-photo-bullet" aria-hidden="true" />
              <span>{point}</span>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (fleetItem.specs) {
    return (
      <div className="fleet-detail-screen fleet-photo-screen">
        <div
          className="fleet-photo-art"
          style={{ backgroundImage: `url(${fleetItem.detailImage})` }}
          aria-hidden="true"
        />

        <h1 className="fleet-photo-title">{fleetItem.detailTitle || fleetItem.label}</h1>

        <div className={`fleet-detail-spec-panel fleet-detail-specs-${fleetItem.specs.length}`}>
          {fleetItem.specs.map((spec) => (
            <div className="fleet-detail-spec" key={`${fleetItem.img}-${spec.label}`}>
              <span className="fleet-detail-spec-icon" aria-hidden="true">
                {spec.icon ? (
                  <img
                    className="fleet-spec-icon-image"
                    src={`./images/fleet-spec-icons/${spec.icon}.png`}
                    alt=""
                    draggable="false"
                  />
                ) : spec.kind === 'ship' ? (
                  <img src={`./images/fleet/${fleetItem.img}.png`} alt="" draggable="false" />
                ) : spec.kind === 'anchor' ? (
                  <Anchor size={94} strokeWidth={1.6} />
                ) : spec.kind === 'settings' ? (
                  <Settings size={94} strokeWidth={1.6} />
                ) : spec.kind === 'users' ? (
                  <Users size={94} strokeWidth={1.6} />
                ) : (
                  <Box size={94} strokeWidth={1.6} />
                )}
              </span>
              <span className="fleet-detail-spec-label">{spec.label}</span>
              <strong className="fleet-detail-spec-value">{spec.value}</strong>
            </div>
          ))}
        </div>

        <button className="fleet-detail-nav fleet-detail-nav-back" onClick={onReturn} aria-label="Return">
          <ChevronLeft size={26} strokeWidth={1.5} />
          <span>RETURN</span>
        </button>
        <button className="fleet-detail-nav fleet-detail-nav-home" onClick={onHome} aria-label="Home">
          <Home size={26} strokeWidth={1.5} />
          <span>HOME</span>
        </button>
      </div>
    );
  }

  if (fleetReferenceImage || !fleetItem.specs) {
    const referenceImage = fleetReferenceImage || fleetItem.detailImage;

    return (
      <div className="fleet-detail-screen fleet-detail-reference-screen">
        <img
          className="fleet-reference-fullscreen"
          src={referenceImage}
          alt={`${fleetItem.detailTitle || fleetItem.label} details`}
          draggable="false"
        />
        <div className="fleet-reference-top-mask" aria-hidden="true" />
        <button className="fleet-detail-nav fleet-detail-nav-back" onClick={onReturn} aria-label="Return">
          <ChevronLeft size={54} strokeWidth={1.5} />
          <span>RETURN</span>
        </button>
        <button className="fleet-detail-nav fleet-detail-nav-home" onClick={onHome} aria-label="Home">
          <Home size={48} strokeWidth={1.5} />
          <span>HOME</span>
        </button>
      </div>
    );
  }

  return (
    <div className="fleet-detail-screen fleet-detail-redesign">
      <div
        className="fleet-detail-art"
        style={{ backgroundImage: `url(${fleetItem.detailImage})` }}
        aria-hidden="true"
      />
      <div className="fleet-detail-top-shade" aria-hidden="true" />

      <button className="fleet-detail-nav fleet-detail-nav-back" onClick={onReturn} aria-label="Return">
        <ChevronLeft size={54} strokeWidth={1.5} />
        <span>RETURN</span>
      </button>
      <button className="fleet-detail-nav fleet-detail-nav-home" onClick={onHome} aria-label="Home">
        <Home size={48} strokeWidth={1.5} />
        <span>HOME</span>
      </button>

      <h1 className="fleet-detail-title">{fleetItem.detailTitle || fleetItem.label}</h1>

      <div className={`fleet-detail-spec-panel fleet-detail-specs-${fleetItem.specs.length}`}>
        {fleetItem.specs.map((spec) => (
          <div className="fleet-detail-spec" key={`${fleetItem.img}-${spec.label}`}>
            <span className="fleet-detail-spec-icon" aria-hidden="true">
              {spec.icon ? (
                <img
                  className="fleet-spec-icon-image"
                  src={`./images/fleet-spec-icons/${spec.icon}.png`}
                  alt=""
                  draggable="false"
                />
              ) : spec.kind === 'ship' ? (
                <img src={`./images/fleet/${fleetItem.img}.png`} alt="" draggable="false" />
              ) : spec.kind === 'anchor' ? (
                <Anchor size={94} strokeWidth={1.6} />
              ) : spec.kind === 'settings' ? (
                <Settings size={94} strokeWidth={1.6} />
              ) : spec.kind === 'users' ? (
                <Users size={94} strokeWidth={1.6} />
              ) : (
                <Box size={94} strokeWidth={1.6} />
              )}
            </span>
            <span className="fleet-detail-spec-label">{spec.label}</span>
            <strong className="fleet-detail-spec-value">{spec.value}</strong>
          </div>
        ))}
      </div>
    </div>
  );
}

const facilities = [
  { label: 'HEAD OFFICE', img: 'head-office', w: 260, detailImage: './images/facility-details/head-office-new.jpeg' },
  { label: 'WAREHOUSE & STORAGE', img: 'warehouse-storage', w: 245, detailImage: './images/facility-details/warehouse-new.jpeg' },
  { label: 'SHIPYARD FACILITY', img: 'shipyard-facility', w: 262, detailImage: './images/facility-details/shipyard-new.jpeg' },
  { label: 'MARINE JETTY', detailTitle: 'JETTY FACILITY', img: 'marine-jetty', w: 255, icon: './images/facilities/marine-jetty-v2.png', detailImage: './images/facility-details/marine-jetty-new.jpeg' },
  { label: 'WORKSHOP & MAINTENANCE', img: 'workshop-maintenance', w: 140, icon: './images/facilities/workshop-maintenance-v2.png', detailImage: './images/facility-details/workshop-new.jpeg' },
  { label: 'TRAVELIFT FACILITY', img: 'travelift-facility', w: 200, detailImage: './images/facility-details/travelift-new.jpeg' },
];

const facilitySpriteIndex = {
  'workshop-maintenance': 0,
  'warehouse-storage': 1,
  'head-office': 2,
  'travelift-facility': 3,
  'shipyard-facility': 4,
  'marine-jetty': 5,
};

function FacilitiesScreen({ onSelectDetail }) {
  return (
    <div className="services-page">
      <div className="facilities-head">
        <h2 className="facilities-title anim">
          LMS ICAD II BASE
        </h2>
        <span className="services-rule facilities-rule anim" />
        <p className="facilities-sub anim">90,000 SQ. M. OF INTEGRATED FACILITIES</p>
        <p className="facilities-hint anim">Explore the facilities below →</p>
      </div>

      <div className="facilities-grid">
        {facilities.map((f) => (
          <button key={f.img} className="facility-item anim" onClick={() => onSelectDetail(f)}>
            <span className="facility-icon-box">
              {f.icon ? (
                <img className="facility-icon" src={f.icon} alt="" draggable="false" />
              ) : (
                <span
                  className={`facility-icon facility-icon-sprite facility-icon-sprite-${facilitySpriteIndex[f.img]}`}
                  aria-hidden="true"
                  style={{ backgroundImage: "url('./images/facilities/facility-icon-sprite.png')" }}
                />
              )}
            </span>
            <span className="facility-label">{f.label}</span>
          </button>
        ))}
      </div>
    </div>
  );
}

const companyStats = [
  { lines: ['BOARD OF DIRECTORS'], img: 'directors', w: 125 },
  { lines: ['MANAGEMENT TEAM'], img: 'management', w: 125 },
  { lines: ['116 +', 'VESSELS'], img: 'vessels', w: 100 },
];

function CompanyScreen({ onOpenBoard, onOpenManagement, onOpenVessels }) {
  return (
    <div className="services-page">
      <div className="company-intro">
        <h1>
          <span>KHALID FARAJ SHIPPING</span>
          <span>L.L.C. – S.P.C.</span>
        </h1>
        <span className="company-intro-rule" aria-hidden="true" />
        <p className="company-intro-copy">
          Khalid Faraj Shipping L.L.C. – S.P.C. is one of Abu Dhabi’s leading privately owned shipowners, vessel operators, and marine service providers. With a versatile fleet of 116 vessels, we deliver reliable maritime solutions across global waters, serving the offshore, oil and gas, dredging, and EPC segments.
        </p>
      </div>
      <div className="company-stats">
        {companyStats.map((s, index) => (
          (() => {
            const interactive = true;
            const onClick = s.img === 'directors' ? onOpenBoard : s.img === 'management' ? onOpenManagement : onOpenVessels;

            return <button
            key={s.img}
            type="button"
            className={`company-stat company-stat-interactive anim`}
            onClick={onClick}
          >
            <span className="company-icon-box">
              {s.img === 'management' ? (
                <Users className="company-management-icon" size={94} strokeWidth={2.4} aria-hidden="true" />
              ) : (
                <span
                  className={`company-icon-sprite company-icon-sprite-${s.img === 'vessels' ? 0 : 2}`}
                  aria-hidden="true"
                  style={{ backgroundImage: "url('./images/company/company-icon-sprite.png')" }}
                />
              )}
            </span>
            <span className="company-stat-label">
              {s.lines.map((line, i) => (
                <span key={i}>{line}</span>
              ))}
            </span>
            <span className="company-bar" />
            </button>;
          })()
        ))}
      </div>
    </div>
  );
}

function CompanySubpageScreen({ title, onReturn, onHome }) {
  return (
    <div className="board-screen company-subpage-screen">
      <button className="fleet-detail-nav fleet-detail-nav-back" onClick={onReturn} aria-label="Return">
        <ChevronLeft size={26} strokeWidth={1.5} />
        <span>RETURN</span>
      </button>
      <button className="fleet-detail-nav fleet-detail-nav-home" onClick={onHome} aria-label="Home">
        <Home size={26} strokeWidth={1.5} />
        <span>HOME</span>
      </button>
      <h1 className="board-title">{title}</h1>
    </div>
  );
}

// VESSELS PAGE: fleet list text taken from vessels.xls (Fleet List 2025).
const vesselsHeader = [
  '',
  '',
  '',
  'FLEET LIST - OWNED AND UNDER MANAGEMENT',
];

const vesselColumns = [
  'Sl. No.',
  'Name of the Vessel',
  'OPCO / SPC / PPE Status',
  'Type of the Vessel',
  'Month and Year Built',
  'LOA (meters)',
  'GRT',
  'NRT',
  'DWCC / DWT',
  'Owners',
];

// [sl, name, note, opco, type, built, loa, grt, nrt, dwt, owner]
const vesselRows = [
  ["1", "\"RAWDAH - 1\"", "", "ASMV / ADNOC SPC / FEP / CEP", "Landing Craft [ LCT ]", "December, 2006", "40", "250", "70", "250", "Liwa Marine Services PJSC"],
  ["2", "\"KENOOZ\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "October, 2016", "46", "499", "144", "500", "Liwa Marine Services PJSC"],
  ["3", "\"MARIYAM-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2023", "46.2", "496", "142", "462.173", "Liwa Marine Services PJSC"],
  ["4", "\"TOYOOF\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "November, 2017", "46.2", "497", "150", "462", "Khalid Faraj Shipping LLC-SPC"],
  ["5", "\"ZEHOOR\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2022", "46.2", "496", "149", "442.9", "Liwa Marine Services PJSC"],
  ["6", "\"AZAH\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "September, 2004", "47", "456", "137", "500", "Liwa Marine Services PJSC"],
  ["7", "\"MEZZON-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "January, 2016", "47", "461", "144", "500", "Liwa Marine Services PJSC"],
  ["8", "\"NORAH-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2022", "47", "499", "148", "458.8", "Liwa Marine Services PJSC"],
  ["9", "\"SALMA\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "April, 2006", "47", "481", "144", "500", "Liwa Marine Services PJSC"],
  ["10", "\"SAMHA\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "September, 2004", "47", "456", "137", "500", "Liwa Marine Services PJSC"],
  ["11", "\"SULTAN II\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "December, 2007", "49.13", "453", "302", "500", "Liwa Marine Services PJSC"],
  ["12", "\"REEM MAY\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "March, 2001", "49.5", "691", "208", "600", "Liwa Marine Services PJSC"],
  ["13", "\"BA'AYA - 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "May, 1989", "50", "499", "264", "630", "Khalid Faraj Shipping LLC-SPC"],
  ["14", "\"ABEER\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2018", "63.25", "499", "148", "360", "Liwa Ship Building"],
  ["15", "\"RAHAF\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2018", "63.25", "499", "148", "360", "Liwa Ship Building"],
  ["16", "\"ASYEH\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "June, 2012", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["17", "\"BUSHRA\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "December, 2018", "64", "929", "278", "1100", "Liwa Marine Services PJSC"],
  ["18", "\"DAYYINAH STAR 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "April, 2006", "64", "679", "203", "900", "Khalid Faraj Shipping LLC-SPC"],
  ["19", "\"JARYEH\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "April, 2014", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["20", "\"KAWAKEB\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "64", "972", "291", "1233", "Liwa Marine Services PJSC"],
  ["21", "\"KHAWLAH-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "December, 2006", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["22", "\"KHWATER\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "February, 2019", "64", "929", "278", "1100", "Liwa Marine Services PJSC"],
  ["23", "\"MARWAH 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "July, 2016", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["24", "\"NAAYEM\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "April, 2014", "64", "929", "278", "1100", "Khalid Faraj Shipping LLC-SPC"],
  ["25", "\"NAGWA 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "January, 2019", "64", "929", "278", "1100", "Liwa Marine Services PJSC"],
  ["26", "\"NATHRAH\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "March, 2016", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["27", "\"SANABEL\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "64", "972", "291", "1233", "Liwa Marine Services PJSC"],
  ["28", "\"SWAYDA\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "April, 2015", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["29", "\"SHEWELEH\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "March, 2016", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["30", "\"TAIBAH-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "December, 2006", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["31", "\"TARFFAH-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "July, 2016", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["32", "\"THURAYA-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "April, 2015", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["33", "\"RAZAN 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "April, 2014", "64", "869", "320", "1100", "Liwa Marine Services PJSC"],
  ["34", "\"WASAYF\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "January, 2020", "64", "929", "278", "1100", "Liwa Marine Services PJSC"],
  ["35", "\"TAMARA 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "January, 2019", "65.48", "1070", "325", "1462", "Liwa Marine Services PJSC"],
  ["36", "\"HAWAYA\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "February, 2010", "65.6", "869", "320", "1400", "Liwa Marine Services PJSC"],
  ["37", "\"WARAYA\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "February, 2010", "65.6", "869", "320", "1400", "Liwa Marine Services PJSC"],
  ["38", "\"HOREYEH\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "March, 2017", "68", "1260", "378", "1800", "Liwa Marine Services PJSC"],
  ["39", "\"JURRY\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1595", "478", "2046", "Liwa Marine Services PJSC"],
  ["40", "\"KAWTHAR 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1537", "461", "2046", "Liwa Marine Services PJSC"],
  ["41", "\"KHAYRAT\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1537", "461", "2046", "Liwa Marine Services PJSC"],
  ["42", "\"RWABEE\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2021", "68", "1304", "391", "1837", "Liwa Marine Services PJSC"],
  ["43", "\"SAJAYA\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1595", "478", "2046", "Liwa Marine Services PJSC"],
  ["44", "\"WARDEH 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "February, 2010", "68", "1418", "426", "1856", "Khalid Faraj Shipping LLC-SPC"],
  ["45", "\"YEAM 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "March, 2017", "68", "1260", "378", "1800", "Liwa Marine Services PJSC"],
  ["46", "\"SAHAYEB\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1438", "431", "1935", "Liwa Marine Services PJSC"],
  ["47", "\"RAWASEE\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1600", "480", "2275", "Liwa Marine Services PJSC"],
  ["48", "\"AYEH\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1595", "478", "2046", "Liwa Marine Services PJSC"],
  ["49", "\"ESHRAQ\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1595", "478", "2046", "Liwa Marine Services PJSC"],
  ["50", "\"ANHAR\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1595", "478", "2046", "Liwa Marine Services PJSC"],
  ["51", "\"ASMA-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "68", "1595", "478", "2046", "Liwa Marine Services PJSC"],
  ["52", "\"ASSILAH\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "69", "1396", "523", "1733", "Liwa Marine Services PJSC"],
  ["53", "\"TAKREEM\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "January, 2019", "78", "2330", "699", "3925", "Liwa Marine Services PJSC"],
  ["54", "\"HETOON\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "78", "1929", "579", "2789", "Liwa Marine Services PJSC"],
  ["55", "\"NADA\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "78", "1929", "579", "2789", "Liwa Marine Services PJSC"],
  ["56", "\"NOUF\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2025", "78", "1929", "579", "2789", "Liwa Marine Services PJSC"],
  ["57", "\"JEDAYEL\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2022", "78.4", "1832", "549", "2362", "Liwa Marine Services PJSC"],
  ["58", "\"JEWAHER 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "January, 2020", "78.4", "1832", "549", "2377", "Liwa Marine Services PJSC"],
  ["59", "\"NASAYEM 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2021", "79.8", "2430", "729", "4000", "Liwa Marine Services PJSC"],
  ["60", "\"JAMELAH - 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2022", "80.5", "1819", "545", "2319", "Liwa Marine Services PJSC"],
  ["61", "\"GADEER\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Landing Craft [ LCT ]", "2023", "99.9", "4593", "1362", "6793", "Liwa Marine Services PJSC"],
  ["62", "\"BARAG 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Offshore Crew Boat", "April, 2014", "33.5", "175", "53", "60 Personnel", "Liwa Marine Services PJSC"],
  ["63", "\"SILS-1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Offshore Crew Boat", "2021", "42", "251", "75", "80 Personnel", "Liwa Marine Services PJSC"],
  ["64", "\"HAQUL 1\"", "", "ASMV/ADNOC SPC/FEP/CEP", "Offshore Crew Boat", "2021", "42", "251", "75", "80 Personnel", "Liwa Marine Services PJSC"],
  ["65", "\"DEBRAN 1\"", "Bollard Pull: 55 T / BHP: 4,400", "ASMV/ADNOC SPC/FEP/CEP", "Anchor Handling / Towing Tug", "2012", "37", "482", "145", "Bow Thruster fitted", "Liwa Marine Services PJSC"],
  ["66", "\"KESSAB 1\"", "Bollard Pull: 55 T / BHP: 4,400", "ASMV/ADNOC SPC/FEP/CEP", "Anchor Handling / Towing Tug", "2011", "37", "482", "145", "Bow Thruster fitted", "Liwa Marine Services PJSC"],
  ["67", "\"MARBAN\"", "Bollard Pull: 15 T / BHP: 1,200", "ASMV/ADNOC SPC/FEP/CEP", "Tug Boat with Towing Hook", "2008", "23.5", "170", "50", "Not Applicable", "Liwa Marine Services PJSC"],
  ["68", "\"RAMASI\"", "Bollard Pull: 27 T / BHP: 2060", "ASMV/ADNOC SPC/FEP/CEP", "Tug Boat with Towing Winch", "2008", "30", "256", "77", "Not Applicable", "Liwa Marine Services PJSC"],
  ["69", "\"SAEED\"", "Bollard Pull : 27 T / BHP 2,060", "ASMV/ADNOC SPC/FEP/CEP", "Tug Boat with Towing Winch", "October, 2005", "31", "297", "89", "Not Applicable", "Liwa Marine Services PJSC"],
  ["70", "\"SARWAN\"", "Bollard Pull: 27 T / BHP: 2,060", "ASMV/ADNOC SPC/FEP/CEP", "Tug Boat with Towing Hook", "2008", "30", "256", "77", "Not Applicable", "Liwa Marine Services PJSC"],
  ["71", "\"LATEER\"", "Bollard Pull: 18 T / BHP: 1,300", "ASMV/ADNOC SPC/FEP/CEP", "Tug Boat with Towing Hook", "2008", "24", "190", "56", "Not Applicable", "Liwa Marine Services PJSC"],
  ["72", "\"SWYHAN 1\"", "Bollard Pull: 18 T / BHP: 1,440", "ASMV/ADNOC SPC/FEP/CEP", "AHT Fitted with \"A\" Frame", "2008", "26", "202", "60", "Not Applicable", "Liwa Marine Services PJSC"],
  ["73", "\"TRAYCE\"", "Bollard Pull: 27/ BHP: 2,060", "ASMV/ADNOC SPC/FEP/CEP", "Tug Boat with Towing Winch", "2011", "28.66", "297", "89", "Not Applicable", "Liwa Marine Services PJSC"],
  ["74", "\"REDOOM\"", "Bollard Pull: 35 T / BHP: 2,800", "ASMV/ADNOC SPC/FEP/CEP", "Tug Boat with Towing Winch", "2008", "31", "320", "90", "Not Applicable", "Liwa Marine Services PJSC"],
  ["75", "\"ARADEH\"", "[ 180' x 56' x 12' ]", "ASMV/ADNOC SPC/FEP/CEP", "Flat Top Cargo Barge", "2008", "52.7", "1100", "300", "2000", "Liwa Marine Services PJSC"],
  ["76", "\"SOHAILEH\"", "[ 180' x 56' x 12' ]", "ASMV/ADNOC SPC/FEP/CEP", "Flat Top Cargo Barge", "2008", "52.7", "1100", "300", "2000", "Liwa Marine Services PJSC"],
  ["77", "\"MOGAIRA 1\"", "[ 180' X 56' X 12' ]", "ASMV/ADNOC SPC/FEP/CEP", "Flat Top Cargo Barge", "2010", "54.86", "1100", "300", "2000", "Liwa Marine Services PJSC"],
  ["78", "\"HARMYAH\"", "[ 250' x 70' x 16' ]", "ASMV/ADNOC SPC/FEP/CEP", "Dumb / Flat Top Barge", "2005", "76.2", "1957", "500", "4600", "Liwa Marine Services PJSC"],
  ["79", "\"L M S - 02\"", "", "10 PAX", "Passenger / Service Boat", "2008", "13.1", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["80", "\"L M S - 03\"", "", "10 PAX", "Passenger / Service Boat", "2025", "13.1", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["81", "\"L M S - 06\"", "", "10 PAX", "Passenger / Service Boat", "2025", "13.1", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["82", "\"L M S - 07\"", "", "10 PAX", "Passenger / Service Boat", "2025", "13.1", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["83", "\"L M S - 11\"", "", "10 PAX", "Passenger / Service Boat", "2025", "13.1", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["84", "\"L M S - 12\"", "", "10 PAX", "Passenger / Service Boat", "2025", "13.1", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["85", "\"L M S - 16\"", "", "10 PAX", "Passenger / Service Boat", "2008", "10.9", "5", "5", "-", "Liwa Marine Services PJSC"],
  ["86", "\"L M S - 18\"", "", "30 PAX", "Passenger / Service Boat", "2017", "13.1", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["87", "\"L M S - 21\"", "", "30 PAX", "Passenger / Service Boat", "2017", "13.1", "10", "10", "-", "Liwa Marine Services PJSC"],
  ["88", "\"L M S - 23\"", "", "30 PAX", "Passenger / Service Boat", "2017", "13.11", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["89", "\"L M S - 24\"", "", "30 PAX", "Passenger / Service Boat", "2017", "13.11", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["90", "\"L M S - 25\"", "", "30 PAX", "Passenger / Service Boat", "2018", "13.11", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["91", "\"L M S - 26\"", "", "30 PAX", "Passenger / Service Boat", "2018", "13.11", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["92", "\"L M S - 27\"", "", "22 PAX", "Passenger / Service Boat", "2018", "13.46", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["93", "\"L M S - 28\"", "", "20 PAX", "Passenger / Service Boat", "2018", "11.82", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["94", "\"L M S - 29\"", "", "20 PAX", "Passenger / Service Boat", "2019", "13", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["95", "\"L M S - 30\"", "", "20 PAX", "Passenger / Service Boat", "2019", "13.3", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["96", "\"L M S - 31\"", "", "30 PAX", "Passenger / Service Boat", "2025", "13.78", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["97", "\"L M S - 32\"", "", "30 PAX", "Passenger / Service Boat", "2022", "13.78", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["98", "\"L M S - 33\"", "", "30 PAX", "Passenger / Service Boat", "2022", "13.71", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["99", "\"L M S - 34\"", "", "30 PAX", "Passenger / Service Boat", "2022", "13.71", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["100", "\"L M S - 35\"", "", "31 PAX", "Passenger / Service Boat", "2022", "13.71", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["101", "\"L M S - 36\"", "", "29 PAX", "Passenger / Service Boat", "2022", "13.71", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["102", "\"L M S - 37\"", "", "30 PAX", "Passenger / Service Boat", "2022", "13.78", "15", "15", "-", "Liwa Marine Services PJSC"],
  ["103", "\"L M S - 38\"", "", "20 PAX", "Passenger / Service Boat", "2019", "11.86", "10", "10", "-", "Liwa Marine Services PJSC"],
  ["104", "\"L M S - 39\"", "", "20 PAX", "Passenger / Service Boat", "2022", "12.1", "5", "5", "-", "Liwa Marine Services PJSC"],
  ["105", "\"L M S - 40\"", "", "20 PAX", "Passenger / Service Boat", "2014", "12.19", "5", "5", "-", "Liwa Marine Services PJSC"],
  ["106", "\"L M S - 42\"", "", "24 PAX", "Passenger / Service Boat", "2023", "16.6", "16", "8", "-", "Liwa Marine Services PJSC"],
  ["107", "\"L M S - 43\"", "", "10 PAX", "Passenger / Service Boat", "2023", "10.25", "14", "8", "-", "Liwa Marine Services PJSC"],
  ["108", "\"L M S - 46\"", "", "10 PAX", "Passenger / Service Boat", "2024", "11.9", "10", "10", "-", "Liwa Marine Services PJSC"],
  ["109", "\"L M S - 49\"", "", "10 PAX", "Passenger / Service Boat", "2023", "11.5", "10", "10", "-", "Liwa Marine Services PJSC"],
  ["110", "\"L M S - 50\"", "", "10 PAX", "Passenger / Service Boat", "2023", "11.5", "10", "10", "-", "Liwa Marine Services PJSC"],
  ["111", "\"L M S - 51\"", "", "10 PAX", "Passenger / Service Boat", "2024", "11.5", "10", "10", "-", "Liwa Marine Services PJSC"],
  ["112", "\"L M S - 52\"", "", "10 PAX", "Passenger / Service Boat", "2024", "11.5", "10", "10", "-", "Liwa Marine Services PJSC"],
  ["113", "\"L M S - 55\"", "", "119 PAX", "Passenger / Service Boat", "2025", "23.3", "135", "135", "-", "Liwa Marine Services PJSC"],
  ["114", "\"L M S - 56\"", "", "10 PAX", "Passenger / Service Boat", "2025", "23.3", "135", "135", "-", "Liwa Marine Services PJSC"],
  ["115", "\"LMS ONE\"", "", "10 PAX", "Passenger / Service Boat", "2025", "23.3", "135", "135", "-", "Liwa Marine Services PJSC"],
  ["116", "\"MOULHIMAH\"", "", "280 PAX", "Passenger Ferry", "2025", "42.39", "468", "153", "TBA", "Liwa Marine Services PJSC"],
];

const vesselNotes = [
  '1. After proposing the firm business, rates, terms and conditons will be negotiated and business is always concluded based on the respective industry approved and or adopted Charter Parties',
  '2. The Vessels Particulars are believed to be correct but without guarantee',
  '3. Charter of each Vessel subject to availability',
  '4. Liwa Marine Services (LMS), Abu Dhabi is an associate of Khalid Faraj Shipping (KFS) and all Vessels owned by LMS are Commercially and Technically managed by KFS',
];

const vesselColumnClass = ['sl', 'name', 'opco', 'type', 'built', 'loa', 'grt', 'nrt', 'dwt', 'owner'];

function VesselsScreen({ onReturn, onHome }) {
  return (
    <div className="board-screen vessels-screen">
      <button className="fleet-detail-nav fleet-detail-nav-back" onClick={onReturn} aria-label="Return">
        <ChevronLeft size={26} strokeWidth={1.5} />
        <span>RETURN</span>
      </button>
      <button className="fleet-detail-nav fleet-detail-nav-home" onClick={onHome} aria-label="Home">
        <Home size={26} strokeWidth={1.5} />
        <span>HOME</span>
      </button>

      <h1 className="board-title">VESSELS</h1>

      <div className="vessels-meta">
        {vesselsHeader.map((line) => (
          <span key={line}>{line}</span>
        ))}
      </div>

      <div className="vessels-row vessels-head">
        {vesselColumns.map((label, i) => (
          <span key={label} className={`vc vc-${vesselColumnClass[i]}`}>{label}</span>
        ))}
      </div>

      <div className="vessels-scroll">
        {vesselRows.map((r) => {
          const [sl, name, note, opco, type, built, loa, grt, nrt, dwt, owner] = r;
          const cells = [sl, name, opco, type, built, loa, grt, nrt, dwt, owner];
          return (
            <div className="vessels-row" key={sl}>
              {cells.map((value, i) => (
                <span key={vesselColumnClass[i]} className={`vc vc-${vesselColumnClass[i]}`}>
                  {value}
                  {vesselColumnClass[i] === 'name' && note ? <small>{note}</small> : null}
                </span>
              ))}
            </div>
          );
        })}

        <div className="vessels-notes">
          <strong>Notes:</strong>
          {vesselNotes.map((n) => (
            <p key={n}>{n}</p>
          ))}
        </div>
      </div>
    </div>
  );
}

const boardMembers = [
  { name: 'KHALFAN FARAJ AL MEHAIRBI', position: 'Chairman', image: './images/company/board/chairman-transparent.png' },
  { name: 'KHALID FARAJ AL MEHAIRBI', position: 'Founder Chairman', image: './images/company/board/founder-chairman-transparent.png' },

  { name: 'MOHAMED FARAJ AL MEHAIRBI', position: 'Managing Director', image: './images/company/board/managing-director-transparent.png' },
];

function BoardScreen({ onReturn, onHome }) {
  return (
    <div className="board-screen">
      <button className="fleet-detail-nav fleet-detail-nav-back" onClick={onReturn} aria-label="Return">
        <ChevronLeft size={26} strokeWidth={1.5} />
        <span>RETURN</span>
      </button>
      <button className="fleet-detail-nav fleet-detail-nav-home" onClick={onHome} aria-label="Home">
        <Home size={26} strokeWidth={1.5} />
        <span>HOME</span>
      </button>

      <h1 className="board-title">BOARD OF DIRECTORS</h1>
      <div className="board-grid">
        {boardMembers.map((member) => (
          <div className="board-member" key={member.image}>
            <img className="board-photo" src={member.image} alt={member.name} draggable="false" />
            <strong>{member.name}</strong>
            <span>{member.position}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

function Background() {
  const ref = useRef(null);

  // If the video file has black bars baked in, measure them and zoom past them.
  // A normal full-frame video gets no extra zoom, so it just fills the screen.
  useEffect(() => {
    const v = ref.current;
    let p = 1; // fraction of the frame width that is real picture

    const apply = () => {
      const A = window.innerWidth / window.innerHeight;
      const F = v.videoWidth / v.videoHeight || 16 / 9;
      const z = A >= F ? 1 / p : Math.max(1, A / (p * F));
      v.style.setProperty('--video-zoom', String(z * 1.04));
    };

    const measure = () => {
      try {
        const c = document.createElement('canvas');
        c.width = 200;
        c.height = 20;
        const ctx = c.getContext('2d');
        ctx.drawImage(v, 0, 0, 200, 20);
        const d = ctx.getImageData(0, 0, 200, 20).data;
        let col = 0;
        for (; col < 100; col++) {
          let sum = 0;
          for (let r = 0; r < 20; r++) {
            const k = (r * 200 + col) * 4;
            sum += d[k] + d[k + 1] + d[k + 2];
          }
          if (sum / 60 > 25) break;
        }
        p = col < 3 ? 1 : 1 - (2 * col) / 200;
      } catch {
        p = 1;
      }
      apply();
    };

    v.addEventListener('loadeddata', measure);
    window.addEventListener('resize', apply);
    if (v.readyState >= 2) measure();
    return () => {
      v.removeEventListener('loadeddata', measure);
      window.removeEventListener('resize', apply);
    };
  }, []);

  return (
    <div className="ocean-scene" aria-hidden="true">
      <video ref={ref} className="ocean-video" autoPlay muted loop playsInline preload="auto">
        <source src="./videos/ocean.mp4" type="video/mp4" />
      </video>
      <div className="ocean-overlay" />
    </div>
  );
}

function IntroScreen({ onEnter }) {
  function handleKeyDown(event) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      onEnter();
    }
  }

  return (
    <button
      className="intro-screen"
      onPointerDown={onEnter}
      onKeyDown={handleKeyDown}
      aria-label="Open home page"
    >
      <video className="intro-video" autoPlay muted loop playsInline preload="auto">
        <source src="./videos/intro.mp4" type="video/mp4" />
      </video>
    </button>
  );
}

function Logo({ small }) {
  return (
    <div className={`logo ${small ? 'logo-small' : ''}`}>
      <img className="logo-exact" src="./images/logo-exact.png" alt="Khalid Faraj Shipping" draggable="false" />
    </div>
  );
}

function HomeScreen({ onSelect }) {
  return (
    <div className="menu-grid">
      {sections.map((s) => (
        <button key={s.id} className="menu-item anim" onClick={() => onSelect(s)}>
          <img className="menu-icon" src={s.image} alt="" draggable="false" />
          <span className="menu-label">{s.title.toUpperCase()}</span>
        </button>
      ))}
    </div>
  );
}

function DetailScreen({ section }) {
  return (
    <div className="detail-card">
      <div className="detail-copy">
        <span className="detail-kicker anim">{section.subtitle}</span>
        <h1 className="anim">{section.title}</h1>
        <p className="anim">{section.description}</p>
        <ul className="detail-list">
          {section.details.map((d, i) => (
            <li className="anim" key={d}>
              <span>0{i + 1}</span>
              {d}
            </li>
          ))}
        </ul>
      </div>
      <div className="detail-visual anim">
        <img src={section.image} alt="" draggable="false" />
      </div>
    </div>
  );
}

export default function App() {
  const [active, setActive] = useState(null);
  const [activeFleet, setActiveFleet] = useState(null);
  const [showBoard, setShowBoard] = useState(false);
  const [companySubpage, setCompanySubpage] = useState(null);
  const [showIntro, setShowIntro] = useState(true);
  const screenRef = useRef(null);
  const stageRef = useRef(null);

  // Fixed 1080x1080 (1:1) design canvas, scaled to fully fit a square LED wall.
  // Keeping all layout coordinates in this logical canvas makes the experience
  // behave consistently on a 3x3 wall and on smaller preview windows.
  useLayoutEffect(() => {
    const fit = () => {
      if (!stageRef.current) return;
      const s = Math.min(window.innerWidth, window.innerHeight) / 1080;
      stageRef.current.style.transform = `translate(-50%, -50%) scale(${s})`;
    };
    fit();
    window.addEventListener('resize', fit);
    return () => window.removeEventListener('resize', fit);
  }, []);

  useLayoutEffect(() => {
    const ctx = gsap.context(() => {
      gsap.fromTo(
        '.anim',
        { y: 26, opacity: 0 },
        { y: 0, opacity: 1, duration: 0.8, stagger: 0.1, ease: 'power3.out' }
      );
    }, screenRef);
    return () => ctx.revert();
  }, [active, activeFleet]);

  useLayoutEffect(() => {
    gsap.fromTo(
      '.top-bar, .logo',
      { y: -18, opacity: 0 },
      { y: 0, opacity: 1, duration: 0.9, stagger: 0.12, ease: 'power3.out' }
    );
  }, []);

  function go(next) {
    const el = screenRef.current;
    if (!el) return setActive(next);
    gsap.to(el, {
      opacity: 0,
      duration: 0.25,
      onComplete: () => {
        setActive(next);
        gsap.to(el, { opacity: 1, duration: 0.3 });
      },
    });
  }

  function openFleet(fleetItem) {
    const el = screenRef.current;
    if (!el) return setActiveFleet(fleetItem);
    gsap.to(el, {
      opacity: 0,
      duration: 0.25,
      onComplete: () => {
        setActiveFleet(fleetItem);
        gsap.to(el, { opacity: 1, duration: 0.3 });
      },
    });
  }

  function closeFleet() {
    setActiveFleet(null);
  }

  function goToIntro() {
    setActiveFleet(null);
    setShowBoard(false);
    setCompanySubpage(null);
    setActive(null);
    setShowIntro(true);
  }

  function openHome() {
    setActiveFleet(null);
    setShowBoard(false);
    setCompanySubpage(null);
    setActive(null);
    setShowIntro(false);
  }

  function openBoard() {
    setShowBoard(true);
  }

  function closeBoard() {
    setShowBoard(false);
  }

  return (
    <main className="website-shell">
      <Background />

      <div className="stage" ref={stageRef}>
        {showIntro ? (
          <IntroScreen onEnter={openHome} />
        ) : (
          <>
            {!activeFleet && !showBoard && (
              <header className="top-bar">
                <button
                  className="bar-button"
                  onClick={() => (active ? go(null) : goToIntro())}
                  aria-label="Return"
                >
                  <ChevronLeft size={26} strokeWidth={1.5} />
                  <span>RETURN</span>
                </button>

                <button className="bar-button" onClick={goToIntro} aria-label="Home">
                  <Home size={24} strokeWidth={1.5} />
                  <span>HOME</span>
                </button>
              </header>
            )}

            {!showBoard && !['services', 'fleet', 'facilities', 'company'].includes(active?.id) && <Logo small={!!active} />}

            <div className="scene-content" ref={screenRef}>
              {showBoard ? (
                <BoardScreen onReturn={closeBoard} onHome={goToIntro} />
              ) : companySubpage === 'vessels' ? (
                <VesselsScreen onReturn={() => setCompanySubpage(null)} onHome={goToIntro} />
              ) : companySubpage ? (
                <CompanySubpageScreen
                  title="MANAGEMENT TEAM"
                  onReturn={() => setCompanySubpage(null)}
                  onHome={goToIntro}
                />
              ) : activeFleet ? (
                <FleetDetailScreen fleetItem={activeFleet} onReturn={closeFleet} onHome={goToIntro} />
              ) : active?.id === 'services' ? (
                <ServicesScreen onSelectDetail={openFleet} />
              ) : active?.id === 'fleet' ? (
                <FleetScreen onSelectFleet={openFleet} />
              ) : active?.id === 'facilities' ? (
                <FacilitiesScreen onSelectDetail={openFleet} />
              ) : active?.id === 'company' ? (
                <CompanyScreen
                  onOpenBoard={openBoard}
                  onOpenManagement={() => setCompanySubpage('management')}
                  onOpenVessels={() => setCompanySubpage('vessels')}
                />
              ) : active ? (
                <DetailScreen section={active} />
              ) : (
                <HomeScreen onSelect={go} />
              )}
            </div>
          </>
        )}
      </div>
    </main>
  );
}