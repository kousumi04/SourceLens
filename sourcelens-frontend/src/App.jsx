import { Routes, Route } from "react-router-dom";
import Sidebar from "./components/Sidebar";
import Dashboard from "./pages/Dashboard";
import Papers from "./pages/Papers";
import Claims from "./pages/Claims";
import Sources from "./pages/Sources";
import Evidence from "./pages/Evidence";
import Assessments from "./pages/Assessments";
import Users from "./pages/Users";
import Assistant from "./pages/Assistant";

export default function App() {
  return (
    <div className="flex h-screen w-full overflow-hidden bg-paper">
      <Sidebar />
      <main className="flex-1 overflow-y-auto px-8 py-8">
        <div className="mx-auto max-w-6xl">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/papers" element={<Papers />} />
            <Route path="/claims" element={<Claims />} />
            <Route path="/sources" element={<Sources />} />
            <Route path="/evidence" element={<Evidence />} />
            <Route path="/assessments" element={<Assessments />} />
            <Route path="/assistant" element={<Assistant />} />
            <Route path="/users" element={<Users />} />
          </Routes>
        </div>
      </main>
    </div>
  );
}
