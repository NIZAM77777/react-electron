// Sharpen existing raster edges at render time without changing source assets.
// The sum-one kernels retain flat colors; alpha and image geometry are preserved.
export default function ImageQualityFilters() {
  return (
    <svg className="image-quality-defs" aria-hidden="true" focusable="false">
      <defs>
        <filter
          id="kfs-photo-clarity"
          x="0"
          y="0"
          width="100%"
          height="100%"
          colorInterpolationFilters="sRGB"
        >
          <feConvolveMatrix
            in="SourceGraphic"
            order="3"
            kernelMatrix="0 -0.08 0 -0.08 1.32 -0.08 0 -0.08 0"
            divisor="1"
            bias="0"
            edgeMode="duplicate"
            preserveAlpha="true"
          />
        </filter>
        <filter
          id="kfs-icon-clarity"
          x="0"
          y="0"
          width="100%"
          height="100%"
          colorInterpolationFilters="sRGB"
        >
          <feConvolveMatrix
            in="SourceGraphic"
            order="3"
            kernelMatrix="0 -0.04 0 -0.04 1.16 -0.04 0 -0.04 0"
            divisor="1"
            bias="0"
            edgeMode="duplicate"
            preserveAlpha="true"
          />
        </filter>
      </defs>
    </svg>
  );
}
