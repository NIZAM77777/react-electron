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

function CompanyScreen({ onOpenBoard }) {
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
            const Stat = s.img === 'directors' ? 'button' : 'div';
            const interactive = s.img === 'directors';

            return <Stat
            key={s.img}
            className={`company-stat ${interactive ? 'company-stat-interactive' : 'company-stat-static'} anim`}
            {...(interactive ? { onClick: onOpenBoard, type: 'button' } : {})}
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
            </Stat>;
          })()
        ))}
      </div>
    </div>
  );
}

const boardMembers = [
  { name: 'KHALFAN FARAJ AL MEHAIRBI', position: 'Chairman', image: './images/company/board/chairman-transparent.png' },
  { name: 'KHALID FARAJ AL MEHAIRBI', position: 'Founder Chairman', image: './images/company/board/founder-chairman-transparent.png' },
  { name: 'VENU KRISHNA PILLAI', position: 'General Manager', image: './images/company/board/general-manager-transparent.png' },
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
    setActive(null);
    setShowIntro(true);
  }

  function openHome() {
    setActiveFleet(null);
    setShowBoard(false);
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
              ) : activeFleet ? (
                <FleetDetailScreen fleetItem={activeFleet} onReturn={closeFleet} onHome={goToIntro} />
              ) : active?.id === 'services' ? (
                <ServicesScreen onSelectDetail={openFleet} />
              ) : active?.id === 'fleet' ? (
                <FleetScreen onSelectFleet={openFleet} />
              ) : active?.id === 'facilities' ? (
                <FacilitiesScreen onSelectDetail={openFleet} />
              ) : active?.id === 'company' ? (
                <CompanyScreen onOpenBoard={openBoard} />
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
