// The signature mark: a camera-aperture / lens-iris built from blades,
// echoing "SourceLens" — examining claims closely, blade by blade.
export default function Aperture({ size = 24, color = "currentColor", blades = 6, open = 0.55, className = "" }) {
  const cx = 50, cy = 50, rOuter = 46, rInner = 46 * (1 - open);
  const polar = (r, a) => [cx + r * Math.cos(a), cy + r * Math.sin(a)];
  const points = [];
  for (let i = 0; i < blades; i++) {
    const segment = (Math.PI * 2) / blades;
    const a0 = i * segment;
    const [ox0, oy0] = polar(rOuter, a0);
    const [ox1, oy1] = polar(rOuter, a0 + segment * 0.86);
    const [ix1, iy1] = polar(rInner, a0 + segment * 1.15);
    const [ix0, iy0] = polar(rInner, a0 + segment * 0.3);
    points.push(`${ox0},${oy0} ${ox1},${oy1} ${ix1},${iy1} ${ix0},${iy0}`);
  }
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 100 100"
      className={className}
      aria-hidden="true"
    >
      <circle cx={cx} cy={cy} r={rOuter + 2} fill="none" stroke={color} strokeOpacity="0.25" strokeWidth="2" />
      {points.map((pts, i) => (
        <polygon key={i} points={pts} fill={color} fillOpacity={0.85} />
      ))}
    </svg>
  );
}
