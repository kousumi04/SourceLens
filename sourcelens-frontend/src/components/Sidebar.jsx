import { NavLink } from "react-router-dom";
import Aperture from "./Aperture";

const NAV = [
  { to: "/", label: "Dashboard", end: true },
  { to: "/papers", label: "Papers" },
  { to: "/claims", label: "Claims" },
  { to: "/sources", label: "Sources" },
  { to: "/evidence", label: "Evidence" },
  { to: "/assessments", label: "Assessments" },
  { to: "/assistant", label: "Assistant" },
  { to: "/users", label: "Users" },
];

export default function Sidebar() {
  return (
    <aside className="flex h-full w-60 shrink-0 flex-col bg-ink text-paper">
      <div className="flex items-center gap-2.5 px-6 py-6">
        <Aperture size={26} color="#B8933E" open={0.5} />
        <div className="leading-tight">
          <p className="font-display text-lg font-semibold tracking-tight">SourceLens</p>
          <p className="text-[11px] uppercase tracking-[0.14em] text-ink-soft">Claim verification</p>
        </div>
      </div>

      <nav className="flex-1 px-3 pt-2">
        {NAV.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              `mb-1 flex items-center gap-2 rounded-md border-l-2 px-3 py-2.5 text-sm transition-colors ${
                isActive
                  ? "border-lens bg-white/5 font-medium text-white"
                  : "border-transparent text-ink-soft hover:bg-white/5 hover:text-white"
              }`
            }
          >
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="border-t border-white/10 px-6 py-4 text-[11px] text-ink-soft">
        <p>SourceLens Team &middot; v1.0</p>
        <p className="mt-0.5">ASP.NET Core &middot; EF Core &middot; SQL Server</p>
      </div>
    </aside>
  );
}
