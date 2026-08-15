import { useEffect, useState } from "react";
import { useAuth as useClerkAuth } from "@clerk/clerk-react";
import {
  ArrowRight,
  Bell,
  BookOpen,
  CalendarDays,
  Check,
  ChevronRight,
  CircleHelp,
  Clock3,
  Code2,
  FileCheck2,
  Flame,
  GraduationCap,
  LayoutDashboard,
  LogOut,
  Menu,
  MessageSquareText,
  Play,
  Search,
  Settings,
  Sparkles,
  Trophy,
  UserRound,
  Users,
  X,
} from "lucide-react";
import {
  Link,
  NavLink,
  Navigate,
  Route,
  Routes,
  useLocation,
  useParams,
} from "react-router-dom";
import { activity, assessments, courses, curriculum } from "./data";
import { authApi } from "@platform/api";
import {
  AnnouncementsPage,
  AuthPage,
  BillingPage,
  CertificatesPage,
  CheckoutPage,
  CourseCatalog,
  CourseDetail,
  ExamPage,
  HomeworkPage,
  JudgeChallengePage,
  JudgePage,
  LiveRoomPage,
  LiveSessionsPage,
  NotificationsPage,
  PricingPage,
  ProfilePage,
  PublicHome,
  SettingsPage,
  StatusPage,
  TeacherAnnouncements,
  TeacherCourseEditor,
  TeacherCourseOverview,
  TeacherCourses,
  TeacherDashboard,
  TeacherExamEditor,
  TeacherExams,
  TeacherHomework,
  TeacherLive,
  TeacherModules,
  TeacherStudents,
} from "./pages";

const courseImages: Record<string, string> = {
  react:
    "https://images.unsplash.com/photo-1633356122544-f134324a6cee?auto=format&fit=crop&w=1200&q=85",
  python:
    "https://images.unsplash.com/photo-1526379095098-d400fd0bf935?auto=format&fit=crop&w=1200&q=85",
  algorithms:
    "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=85",
  dotnet:
    "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=1200&q=85",
};

const navItems = [
  { to: "/dashboard", label: "Overview", icon: LayoutDashboard },
  { to: "/courses", label: "My learning", icon: BookOpen },
  { to: "/assessments", label: "Assessments", icon: FileCheck2 },
  { to: "/judge", label: "Code challenges", icon: Code2 },
  { to: "/schedule", label: "Schedule", icon: CalendarDays },
];

const teacherNavItems = [
  { to: "/teacher/dashboard", label: "Overview", icon: LayoutDashboard },
  { to: "/teacher/courses", label: "Courses", icon: BookOpen },
  { to: "/teacher/exams", label: "Exams", icon: FileCheck2 },
  { to: "/teacher/homework", label: "Homework", icon: GraduationCap },
  { to: "/teacher/students", label: "Students", icon: Users },
  { to: "/teacher/live", label: "Live sessions", icon: CalendarDays },
  { to: "/teacher/announcements", label: "Announcements", icon: Bell },
];

function Shell() {
  const { signOut } = useClerkAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const [searchOpen, setSearchOpen] = useState(false);
  const location = useLocation();
  const role = location.pathname.startsWith("/teacher") ? "Teacher" : "Student";
  const activeNavItems = role === "Teacher" ? teacherNavItems : navItems;

  return (
    <div className="app-shell">
      <aside className={`sidebar ${menuOpen ? "sidebar-open" : ""}`}>
        <div className="brand-row">
          <Link className="brand" to="/dashboard" onClick={() => setMenuOpen(false)}>
            <span className="brand-mark"><Code2 size={20} /></span>
            <span>CODEAN</span>
          </Link>
          <button className="icon-button sidebar-close" onClick={() => setMenuOpen(false)} aria-label="Close menu">
            <X size={19} />
          </button>
        </div>

        <nav className="primary-nav" aria-label="Primary navigation">
          <p className="nav-label">Workspace</p>
          {activeNavItems.map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} onClick={() => setMenuOpen(false)} className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
              <Icon size={19} />
              <span>{label}</span>
            </NavLink>
          ))}
          {role === "Student" && <><p className="nav-label nav-label-spaced">Connect</p><NavLink className="nav-item" to="/announcements"><Bell size={19} /><span>Announcements</span></NavLink><NavLink className="nav-item" to="/live"><Users size={19} /><span>Live sessions</span></NavLink><NavLink className="nav-item" to="/notifications"><MessageSquareText size={19} /><span>Notifications</span><span className="nav-count">3</span></NavLink></>}
        </nav>

        <div className="sidebar-foot">
          <div className="streak-panel">
            <span className="streak-icon"><Flame size={18} /></span>
            <div><strong>7 day streak</strong><span>Keep the momentum</span></div>
          </div>
          <a className="nav-item" href="#help"><CircleHelp size={19} /><span>Help center</span></a>
          <NavLink className="nav-item" to="/settings"><Settings size={19} /><span>Settings</span></NavLink>
          <Link className="profile-strip" to={role === "Student" ? "/profile" : "/teacher/dashboard"}>
            <span className="avatar">NH</span>
            <div><strong>{role === "Teacher" ? "Maya Hassan" : "Nadia Hassan"}</strong><span>{role}</span></div>
            <ChevronRight size={17} />
          </Link>
          <button
            className="nav-item sign-out-button"
            type="button"
            onClick={() => { void signOut().then(() => window.location.assign("/auth/login")); }}
          >
            <LogOut size={19} />
            <span>Sign out</span>
          </button>
        </div>
      </aside>

      {menuOpen && <button className="sidebar-backdrop" onClick={() => setMenuOpen(false)} aria-label="Close menu" />}

      <div className="workspace">
        <header className="topbar">
          <button className="icon-button mobile-menu" onClick={() => setMenuOpen(true)} aria-label="Open menu"><Menu size={21} /></button>
          <button className={`search-box ${searchOpen ? "search-open" : ""}`} onClick={() => setSearchOpen(true)}>
            <Search size={18} />
            <input aria-label="Search courses" placeholder="Search courses, lessons, or topics" onBlur={() => setSearchOpen(false)} />
            <kbd>Ctrl K</kbd>
          </button>
          <div className="topbar-actions">
            <button className="icon-button notification-button" aria-label="Notifications"><Bell size={20} /><span /></button>
            <Link className="top-avatar" to={role === "Student" ? "/profile" : "/teacher/dashboard"} aria-label="Open profile">NH</Link>
          </div>
        </header>
        <main className="main-content">
          <Routes>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/courses" element={<Courses />} />
            <Route path="/courses/:courseId" element={<CourseWorkspace />} />
            <Route path="/assessments" element={<Assessments />} />
            <Route path="/schedule" element={<Schedule />} />
            <Route path="/announcements" element={<AnnouncementsPage />} />
            <Route path="/billing" element={<BillingPage />} />
            <Route path="/certificates" element={<CertificatesPage />} />
            <Route path="/exams/:examId" element={<ExamPage />} />
            <Route path="/homework/:homeworkId" element={<HomeworkPage />} />
            <Route path="/judge" element={<JudgePage />} />
            <Route path="/judge/:challengeId" element={<JudgeChallengePage />} />
            <Route path="/live" element={<LiveSessionsPage />} />
            <Route path="/live/:sessionId" element={<LiveRoomPage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/teacher/dashboard" element={<TeacherDashboard />} />
            <Route path="/teacher/courses" element={<TeacherCourses />} />
            <Route path="/teacher/courses/new" element={<TeacherCourseEditor />} />
            <Route path="/teacher/courses/:id" element={<TeacherCourseOverview />} />
            <Route path="/teacher/courses/:id/edit" element={<TeacherCourseEditor />} />
            <Route path="/teacher/courses/:id/modules" element={<TeacherModules />} />
            <Route path="/teacher/exams" element={<TeacherExams />} />
            <Route path="/teacher/exams/new" element={<TeacherExamEditor />} />
            <Route path="/teacher/homework" element={<TeacherHomework />} />
            <Route path="/teacher/students" element={<TeacherStudents />} />
            <Route path="/teacher/live" element={<TeacherLive />} />
            <Route path="/teacher/announcements" element={<TeacherAnnouncements />} />
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </main>
      </div>
    </div>
  );
}

function PageHeading({ eyebrow, title, copy }: { eyebrow: string; title: string; copy: string }) {
  return (
    <div className="page-heading">
      <div><p className="eyebrow">{eyebrow}</p><h1>{title}</h1><p>{copy}</p></div>
      <div className="heading-date"><CalendarDays size={18} /><span>Wednesday, August 12</span></div>
    </div>
  );
}

function Dashboard() {
  const featured = courses[0];
  return (
    <>
      <PageHeading eyebrow="Student workspace" title="Good morning, Nadia" copy="Pick up where you left off and keep your weekly goal within reach." />

      <section className="dashboard-grid">
        <div className="dashboard-main">
          <article className="continue-panel">
            <img src={courseImages[featured.id]} alt="React code on a laptop screen" />
            <div className="continue-overlay" />
            <div className="continue-content">
              <span className="pill pill-light"><Play size={13} fill="currentColor" /> Continue learning</span>
              <p>{featured.category} · Lesson 17 of {featured.lessons}</p>
              <h2>State management patterns that scale</h2>
              <div className="continue-meta"><span><Clock3 size={15} /> 24 min</span><span>{featured.progress}% complete</span></div>
              <div className="progress-track progress-on-dark"><span style={{ width: `${featured.progress}%` }} /></div>
              <Link className="button button-light" to={`/courses/${featured.id}`}>Resume lesson <ArrowRight size={17} /></Link>
            </div>
          </article>

          <section className="section-block">
            <div className="section-heading"><div><p className="eyebrow">Active courses</p><h2>Your learning</h2></div><Link to="/courses">View all <ArrowRight size={15} /></Link></div>
            <div className="course-row">
              {courses.slice(1, 4).map((course) => <CourseCard key={course.id} course={course} compact />)}
            </div>
          </section>
        </div>

        <aside className="dashboard-rail">
          <section className="stat-grid">
            <div className="stat-cell"><span className="stat-icon amber"><Flame size={19} /></span><strong>7</strong><span>Day streak</span></div>
            <div className="stat-cell"><span className="stat-icon green"><Clock3 size={19} /></span><strong>12.5h</strong><span>This week</span></div>
            <div className="stat-cell"><span className="stat-icon blue"><Trophy size={19} /></span><strong>8</strong><span>Certificates</span></div>
            <div className="stat-cell"><span className="stat-icon coral"><FileCheck2 size={19} /></span><strong>92%</strong><span>Avg. score</span></div>
          </section>

          <section className="rail-section">
            <div className="section-heading compact"><div><p className="eyebrow">Coming up</p><h2>Today</h2></div><Link to="/schedule"><CalendarDays size={17} /></Link></div>
            <div className="timeline-list">
              <div className="timeline-item"><time>11:00</time><span className="timeline-dot live" /><div><strong>Live code review</strong><span>Modern React Engineering</span><em>Starts in 42 min</em></div></div>
              <div className="timeline-item"><time>15:30</time><span className="timeline-dot" /><div><strong>Algorithms office hours</strong><span>Problem solving cohort</span></div></div>
            </div>
          </section>

          <section className="rail-section activity-section">
            <div className="section-heading compact"><div><p className="eyebrow">Recent</p><h2>Activity</h2></div></div>
            <div className="activity-list">
              {activity.slice(0, 3).map((item, index) => <div className="activity-item" key={item}><span><Check size={14} /></span><p>{item}<small>{index + 1}h ago</small></p></div>)}
            </div>
          </section>
        </aside>
      </section>
    </>
  );
}

type Course = (typeof courses)[number];

function CourseCard({ course, compact = false }: { course: Course; compact?: boolean }) {
  return (
    <Link className={`course-card ${compact ? "course-card-compact" : ""}`} to={`/courses/${course.id}`}>
      <div className="course-image"><img src={courseImages[course.id]} alt="" /><span>{course.level}</span></div>
      <div className="course-card-body">
        <p>{course.category}</p><h3>{course.title}</h3><span className="instructor">{course.instructor}</span>
        <div className="course-stats"><span><BookOpen size={14} /> {course.lessons} lessons</span><span><Clock3 size={14} /> {course.duration}</span></div>
        <div className="progress-track"><span style={{ width: `${course.progress}%`, background: course.color }} /></div>
        <div className="progress-label"><span>{course.progress ? `${course.progress}% complete` : "Not started"}</span><ChevronRight size={16} /></div>
      </div>
    </Link>
  );
}

function Courses() {
  const [query, setQuery] = useState("");
  const filtered = courses.filter((course) => `${course.title} ${course.category}`.toLowerCase().includes(query.toLowerCase()));
  return (
    <>
      <PageHeading eyebrow="Course library" title="My learning" copy="Track active courses, revisit completed lessons, and start something new." />
      <div className="toolbar">
        <label className="filter-search"><Search size={17} /><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Filter your courses" /></label>
        <div className="segmented"><button className="selected">All courses</button><button>In progress</button><button>Completed</button></div>
      </div>
      <div className="course-grid">{filtered.map((course) => <CourseCard course={course} key={course.id} />)}</div>
      {filtered.length === 0 && <div className="empty-state"><Search size={28} /><h2>No matching courses</h2><p>Try a different course title or category.</p></div>}
    </>
  );
}

function CourseWorkspace() {
  const { courseId } = useParams();
  const course = courses.find((item) => item.id === courseId) ?? courses[0];
  const [activeLesson, setActiveLesson] = useState("State and events");
  return (
    <div className="learning-layout">
      <section className="lesson-stage">
        <div className="course-breadcrumb"><Link to="/courses">My learning</Link><ChevronRight size={15} /><span>{course.title}</span></div>
        <div className="lesson-player">
          <img src={courseImages[course.id]} alt="Course lesson workspace" />
          <button aria-label="Play lesson"><Play size={26} fill="currentColor" /></button>
          <span className="player-duration">24:18</span>
        </div>
        <div className="lesson-heading"><div><p className="eyebrow">Module 2 · Core concepts</p><h1>{activeLesson}</h1><p>Build a practical mental model, then apply it in a guided production example.</p></div><button className="button button-primary">Mark complete <Check size={17} /></button></div>
        <div className="lesson-tabs"><button className="active">Overview</button><button>Resources</button><button>Discussion</button><button>Notes</button></div>
        <article className="lesson-copy"><h2>What you will learn</h2><p>In this lesson, you will connect component state, user events, and derived values into a predictable data flow. The examples focus on decisions you will make in real product code.</p><div className="learning-points"><span><Check size={15} /> Choose the right state owner</span><span><Check size={15} /> Separate events from effects</span><span><Check size={15} /> Avoid duplicated derived state</span></div></article>
      </section>
      <aside className="curriculum-panel">
        <div className="curriculum-head"><p className="eyebrow">Course curriculum</p><h2>{course.title}</h2><div className="progress-track"><span style={{ width: `${course.progress}%`, background: course.color }} /></div><span>{course.progress}% complete · {course.lessons} lessons</span></div>
        {curriculum.map((module, moduleIndex) => <div className="module" key={module.title}><div className="module-title"><span>{moduleIndex + 1}</span><div><strong>{module.title}</strong><small>{module.lessons.length} lessons</small></div></div>{module.lessons.map((lesson, lessonIndex) => <button className={activeLesson === lesson ? "lesson-link active" : "lesson-link"} onClick={() => setActiveLesson(lesson)} key={lesson}><span className="lesson-status">{moduleIndex === 0 || (moduleIndex === 1 && lessonIndex === 0) ? <Check size={13} /> : lessonIndex + 1}</span><span>{lesson}<small>{18 + lessonIndex * 4} min</small></span>{activeLesson === lesson && <Play size={14} fill="currentColor" />}</button>)}</div>)}
      </aside>
    </div>
  );
}

function Assessments() {
  return (
    <>
      <PageHeading eyebrow="Knowledge checks" title="Assessments" copy="Review upcoming exams, submitted homework, and coding challenges." />
      <div className="assessment-summary"><div><span className="stat-icon blue"><FileCheck2 size={20} /></span><p><strong>3</strong><span>Upcoming exams</span></p></div><div><span className="stat-icon green"><Check size={20} /></span><p><strong>12</strong><span>Completed tasks</span></p></div><div><span className="stat-icon amber"><Trophy size={20} /></span><p><strong>1,840</strong><span>Challenge points</span></p></div></div>
      <div className="assessment-columns">
        <AssessmentList title="Exams" eyebrow="Scheduled" items={assessments.exams} icon={FileCheck2} action="View exam" href={(index) => `/exams/${index + 1}`} />
        <AssessmentList title="Homework" eyebrow="Course work" items={assessments.homework} icon={BookOpen} action="Open task" href={(index) => `/homework/${index + 1}`} />
        <AssessmentList title="Code challenges" eyebrow="Practice" items={assessments.challenges} icon={Code2} action="Solve" href={(index) => `/judge/${["two-sum", "valid-parentheses", "merge-intervals", "lru-cache"][index]}`} />
      </div>
    </>
  );
}

function AssessmentList({ title, eyebrow, items, icon: Icon, action, href }: { title: string; eyebrow: string; items: string[]; icon: typeof Code2; action: string; href: (index: number) => string }) {
  return <section className="assessment-list"><div className="section-heading compact"><div><p className="eyebrow">{eyebrow}</p><h2>{title}</h2></div></div>{items.map((item, index) => <article key={item}><span className="assessment-icon"><Icon size={18} /></span><div><h3>{item}</h3><p>{index === 0 ? "Due tomorrow" : `${index + 2} days remaining`}</p></div><Link to={href(index)}>{action}<ChevronRight size={15} /></Link></article>)}</section>;
}

function Schedule() {
  const days = ["Mon 10", "Tue 11", "Wed 12", "Thu 13", "Fri 14"];
  return (
    <>
      <PageHeading eyebrow="Learning calendar" title="Schedule" copy="Plan live sessions, deadlines, and focused study blocks in one place." />
      <div className="week-strip">{days.map((day, index) => <button className={index === 2 ? "active" : ""} key={day}><span>{day.split(" ")[0]}</span><strong>{day.split(" ")[1]}</strong>{index === 2 && <em>Today</em>}</button>)}</div>
      <section className="schedule-board">
        <div className="schedule-time"><span>09:00</span><span>11:00</span><span>13:00</span><span>15:00</span><span>17:00</span></div>
        <div className="schedule-events">
          <article className="schedule-event event-blue"><span>10:00 - 11:15</span><h3>React architecture workshop</h3><p><UserRound size={14} /> Maya Hassan · Live room 2</p><button>Join session <ArrowRight size={15} /></button></article>
          <article className="schedule-event event-green"><span>13:00 - 14:00</span><h3>Focused study block</h3><p><BookOpen size={14} /> Python Foundations</p></article>
          <article className="schedule-event event-coral"><span>15:30 - 16:15</span><h3>Algorithms office hours</h3><p><UserRound size={14} /> Lina Nasser · Cohort room</p></article>
        </div>
        <aside className="goal-panel"><span className="stat-icon amber"><Sparkles size={20} /></span><p className="eyebrow">Weekly goal</p><h2>8 of 10 hours</h2><div className="progress-track"><span style={{ width: "80%" }} /></div><p>Two more focused hours will complete this week's target.</p><button className="button button-primary">Add study block</button></aside>
      </section>
    </>
  );
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<PublicHome />} />
      <Route path="/catalog" element={<CourseCatalog />} />
      <Route path="/catalog/:courseId" element={<CourseDetail />} />
      <Route path="/pricing" element={<PricingPage />} />
      {/* Clerk's path-based flow uses nested URLs for verification and recovery steps. */}
      <Route path="/auth/:mode/*" element={<AuthPage />} />
      <Route path="/checkout" element={<CheckoutPage />} />
      <Route path="/checkout/success" element={<StatusPage status="success" />} />
      <Route path="/unauthorized" element={<StatusPage status="unauthorized" />} />
      <Route path="/forbidden" element={<StatusPage status="forbidden" />} />
      <Route path="/maintenance" element={<StatusPage status="maintenance" />} />
      <Route path="/admin/*" element={<AdminRedirect />} />
      <Route path="/*" element={<AuthenticatedShell />} />
    </Routes>
  );
}

function AuthenticatedShell() {
  const { isLoaded, isSignedIn, getToken } = useClerkAuth();
  const [verification, setVerification] = useState<"checking" | "authenticated" | "failed">("checking");
  const [error, setError] = useState("");
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    if (!isLoaded) return;
    if (!isSignedIn) {
      setVerification("failed");
      setError("Clerk could not create an authenticated session. Please sign in again.");
      return;
    }

    let cancelled = false;
    setVerification("checking");
    setError("");

    const verifySession = async () => {
      try {
        const token = await getToken();
        if (!token) throw new Error("Clerk did not return a session token. Please sign in again.");
        await authApi.getCurrentUser(token);
        if (!cancelled) setVerification("authenticated");
      } catch (caught) {
        if (cancelled) return;
        if (isApiError(caught)) {
          const status = caught.response?.status;
          const problem = caught.response?.data;
          const backendMessage = problem?.detail ?? problem?.title;
          if (status === 401) setError(backendMessage ?? "The backend rejected your Clerk session. Please sign in again.");
          else if (status === 403) setError(backendMessage ?? "Your account does not have permission to access this workspace.");
          else if (status && status >= 500) setError(backendMessage ?? "The backend could not create or load your account. Please try again.");
          else if (!caught.response) setError("The authentication service could not be reached. Check your connection and try again.");
          else setError(backendMessage ?? `Session verification failed (${status}).`);
        } else {
          setError(caught instanceof Error ? caught.message : "Session verification failed. Please try again.");
        }
        setVerification("failed");
      }
    };

    void verifySession();
    return () => { cancelled = true; };
  }, [attempt, getToken, isLoaded, isSignedIn]);

  if (!isLoaded || verification === "checking") return <main className="status-page"><section><p className="eyebrow">CODEAN workspace</p><h1>Verifying your session...</h1><p>Connecting your Clerk session to your CODEAN account.</p></section></main>;
  if (verification === "failed") return <main className="status-page"><section><p className="eyebrow">Authentication error</p><h1>We could not verify your session</h1><p role="alert">{error}</p><div className="status-actions">{isSignedIn && <button className="button button-primary" onClick={() => setAttempt((value) => value + 1)}>Try again</button>}<Link className="button button-secondary" to="/auth/login">Return to sign in</Link></div></section></main>;
  return <Shell />;
}

function isApiError(error: unknown): error is {
  response?: { status?: number; data?: { detail?: string; title?: string } };
} {
  return typeof error === "object" && error !== null && "isAxiosError" in error;
}

function AdminRedirect() {
  window.location.replace(import.meta.env.VITE_ADMIN_URL ?? "http://localhost:5174");
  return <main className="status-page"><section><p className="eyebrow">Admin console</p><h1>Opening secure administration...</h1></section></main>;
}
