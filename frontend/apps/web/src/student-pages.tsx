import { FormEvent, ReactNode, useEffect, useMemo, useState } from "react";
import { useAuth, useClerk, useUser } from "@clerk/clerk-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  ArrowLeft,
  ArrowRight,
  BookOpen,
  Camera,
  CalendarDays,
  Check,
  CheckCircle2,
  ChevronRight,
  Clock3,
  Code2,
  FileCheck2,
  FileText,
  GraduationCap,
  Mail,
  MessageSquareText,
  Pencil,
  Phone,
  Play,
  Search,
  Send,
  ShieldCheck,
  School,
  Sparkles,
  Trophy,
  UserRound,
  Users,
  Video,
} from "lucide-react";
import {
  announcementsApi,
  authApi,
  coursesApi,
  examsApi,
  homeworkApi,
  judgeApi,
  liveApi,
  learningApi,
  notificationsApi,
  paymentsApi,
  usersApi,
} from "@platform/api";
import type {
  AnnouncementResponse,
  CourseDetailResponse,
  CourseEnrollmentResponse,
  CourseProgressResponse,
  CourseResponse,
  CurrentUserResponse,
  ExamResponse,
  HomeworkResponse,
  LiveSessionResponse,
  CodingChallengeResponse,
} from "@platform/contracts";

const fallbackImages = [
  "https://images.unsplash.com/photo-1633356122544-f134324a6cee?auto=format&fit=crop&w=1200&q=85",
  "https://images.unsplash.com/photo-1526379095098-d400fd0bf935?auto=format&fit=crop&w=1200&q=85",
  "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=85",
  "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=1200&q=85",
];

type Icon = typeof Users;

function messageFor(error: unknown, fallback: string) {
  if (error && typeof error === "object" && "response" in error) {
    const response = (error as { response?: { data?: { detail?: string; title?: string } } }).response;
    return response?.data?.detail ?? response?.data?.title ?? fallback;
  }
  return error instanceof Error ? error.message : fallback;
}

function dateLabel(value: string | Date) {
  return new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(new Date(value));
}

function dateTimeLabel(value: string | Date) {
  return new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
}

function courseImage(course: Pick<CourseResponse, "thumbnail" | "id">) {
  return course.thumbnail || fallbackImages[course.id % fallbackImages.length];
}

function StudentPageHeader({ eyebrow, title, copy, action }: { eyebrow: string; title: string; copy: string; action?: ReactNode }) {
  return <div className="page-heading"><div><p className="eyebrow">{eyebrow}</p><h1>{title}</h1><p>{copy}</p></div><div className="heading-actions">{action}<span className="heading-date"><CalendarDays size={17} /> {dateLabel(new Date())}</span></div></div>;
}

function LoadingState({ label = "Loading from CODEAN..." }: { label?: string }) {
  return <div className="empty-state"><span className="loading-spinner" aria-hidden="true" /><p>{label}</p></div>;
}

function ErrorState({ message, retry }: { message: string; retry?: () => void }) {
  return <div className="empty-state"><FileText size={28} /><h2>We could not load this data</h2><p>{message}</p>{retry && <button className="button button-primary" onClick={retry}>Try again</button>}</div>;
}

function EmptyState({ title, copy, action }: { title: string; copy: string; action?: ReactNode }) {
  return <div className="empty-state"><Sparkles size={28} /><h2>{title}</h2><p>{copy}</p>{action}</div>;
}

function Metric({ icon: IconComponent, label, value, tone = "blue" }: { icon: Icon; label: string; value: string; tone?: string }) {
  return <div className="metric-box"><span className={`stat-icon ${tone}`}><IconComponent size={19} /></span><div><strong>{value}</strong><span>{label}</span></div></div>;
}

function EnrolledCourseCard({ enrollment }: { enrollment: CourseEnrollmentResponse }) {
  const progress = enrollment.progress?.overallCompletionPercentage ?? 0;
  const lessonCount = enrollment.progress?.totalLessons ?? 0;
  return <Link className="enrolled-course-card" to={`/courses/${enrollment.courseId}`}>
    <div className="enrolled-course-image"><img src={enrollment.courseThumbnail || fallbackImages[enrollment.courseId % fallbackImages.length]} alt="" /><span>{progress >= 100 ? "Completed" : progress > 0 ? "In progress" : "Ready to start"}</span></div>
    <div className="enrolled-course-body"><div className="enrolled-course-heading"><div><p className="eyebrow">{enrollment.category || "Course"}</p><h3>{enrollment.courseTitle}</h3></div><ArrowRight size={17} /></div><p className="enrolled-course-teacher">With {enrollment.teacherName || "your instructor"}</p><div className="enrolled-course-meta"><span><BookOpen size={14} /> {lessonCount} lessons</span><span>{Math.round(progress)}% complete</span></div><div className="progress-track"><span style={{ width: `${Math.max(0, Math.min(100, progress))}%` }} /></div><div className="enrolled-course-footer"><span>{progress >= 100 ? "Course completed" : progress > 0 ? "Continue learning" : "Start your first lesson"}</span><small>{enrollment.accessType === "Free" ? "Free access" : "Paid access"}</small></div></div>
  </Link>;
}

export function StudentDashboard({ currentUser }: { currentUser: CurrentUserResponse }) {
  const [enrollments, setEnrollments] = useState<CourseEnrollmentResponse[]>([]);
  const [sessions, setSessions] = useState<LiveSessionResponse[]>([]);
  const [notifications, setNotifications] = useState<{ id: number; title: string; body: string; createdAt: string }[]>([]);
  const [state, setState] = useState<"loading" | "ready" | "error">("loading");
  const [error, setError] = useState("");

  const load = async () => {
    setState("loading");
    try {
      const [enrollmentResult, sessionResult, notificationResult] = await Promise.all([
        learningApi.getEnrolledCourses(),
        liveApi.getUpcoming(),
        notificationsApi.getAll({ pageSize: 5 }),
      ]);
      setEnrollments(enrollmentResult);
      setSessions(sessionResult);
      setNotifications(notificationResult.items);
      setState("ready");
    } catch (cause) {
      setError(messageFor(cause, "The student workspace is temporarily unavailable."));
      setState("error");
    }
  };

  useEffect(() => { void load(); }, []);
  if (state === "loading") return <LoadingState />;
  if (state === "error") return <ErrorState message={error} retry={() => void load()} />;

  const featured = enrollments.find((item) => (item.progress?.overallCompletionPercentage ?? 0) > 0 && (item.progress?.overallCompletionPercentage ?? 0) < 100) ?? enrollments.find((item) => (item.progress?.overallCompletionPercentage ?? 0) < 100) ?? enrollments[0];
  const featuredProgress = featured?.progress?.overallCompletionPercentage ?? 0;
  const completedCourses = enrollments.filter((item) => (item.progress?.overallCompletionPercentage ?? 0) >= 100).length;

  return <>
    <StudentPageHeader eyebrow="Student workspace" title={`Welcome, ${currentUser.fullName || "learner"}`} copy="Your learning activity comes directly from your CODEAN account." />
    <div className="metric-row"><Metric icon={BookOpen} label="My courses" value={String(enrollments.length)} /><Metric icon={CheckCircle2} label="Completed courses" value={String(completedCourses)} tone="green" /><Metric icon={Video} label="Upcoming sessions" value={String(sessions.length)} tone="amber" /><Metric icon={MessageSquareText} label="Recent updates" value={String(notifications.length)} tone="coral" /></div>
    {!featured ? <EmptyState title="Your learning workspace is ready" copy="You are not enrolled in a course yet. Explore the catalog and choose a free course or complete checkout for a paid course." action={<Link className="button button-primary" to="/catalog">Browse course catalog</Link>} /> : <section className="dashboard-grid">
      <div className="dashboard-main"><article className="continue-panel"><img src={featured.courseThumbnail || fallbackImages[featured.courseId % fallbackImages.length]} alt="" /><div className="continue-overlay" /><div className="continue-content"><span className="pill pill-light"><Play size={13} fill="currentColor" /> {featuredProgress ? "Continue learning" : "Start learning"}</span><p>{featured.category} · {featured.progress?.totalLessons ?? 0} lessons</p><h2>{featured.courseTitle}</h2><div className="continue-meta"><span><Clock3 size={15} /> {featured.teacherName || "Your instructor"}</span><span>{Math.round(featuredProgress)}% complete</span></div><div className="progress-track progress-on-dark"><span style={{ width: `${featuredProgress}%` }} /></div><Link className="button button-light" to={`/courses/${featured.courseId}`}>{featuredProgress ? "Resume course" : "Open course"} <ArrowRight size={17} /></Link></div></article><section className="section-block"><div className="section-heading"><div><p className="eyebrow">Your enrolled courses</p><h2>Keep your momentum</h2></div><Link to="/courses">View all <ArrowRight size={15} /></Link></div><div className="enrolled-course-grid">{enrollments.slice(0, 3).map((enrollment) => <EnrolledCourseCard key={enrollment.id} enrollment={enrollment} />)}</div></section></div>
      <aside className="dashboard-rail"><section className="rail-section"><div className="section-heading compact"><div><p className="eyebrow">Coming up</p><h2>Live sessions</h2></div><Link to="/schedule"><CalendarDays size={17} /></Link></div>{sessions.length ? <div className="timeline-list">{sessions.slice(0, 3).map((session) => <div className="timeline-item" key={session.id}><time>{new Date(session.startTime).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</time><span className="timeline-dot live" /><div><strong>{session.title}</strong><span>{dateTimeLabel(session.startTime)}</span><em>{session.status}</em></div></div>)}</div> : <p className="muted-copy">No upcoming sessions scheduled.</p>}</section><section className="rail-section activity-section"><div className="section-heading compact"><div><p className="eyebrow">Recent</p><h2>Notifications</h2></div><Link to="/notifications">View all</Link></div>{notifications.length ? <div className="activity-list">{notifications.slice(0, 3).map((item) => <div className="activity-item" key={item.id}><span><Check size={14} /></span><p>{item.title}<small>{dateLabel(item.createdAt)}</small></p></div>)}</div> : <p className="muted-copy">No recent updates.</p>}</section></aside>
    </section>}
  </>;
}

export function StudentCourses() {
  const [items, setItems] = useState<CourseEnrollmentResponse[]>([]); const [query, setQuery] = useState(""); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = () => { setLoading(true); void learningApi.getEnrolledCourses().then(setItems).catch((cause) => setError(messageFor(cause, "Unable to load your enrolled courses."))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []);
  const filtered = useMemo(() => items.filter((course) => `${course.courseTitle} ${course.category} ${course.teacherName}`.toLowerCase().includes(query.toLowerCase())), [items, query]);
  return <><StudentPageHeader eyebrow="Your learning" title="My courses" copy="Everything you have access to, with progress saved to your CODEAN account." /><div className="toolbar"><label className="filter-search"><Search size={17} /><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search your enrolled courses" /></label><Link className="button button-outline" to="/catalog">Explore catalog</Link></div>{loading ? <LoadingState /> : error ? <ErrorState message={error} retry={load} /> : filtered.length ? <div className="enrolled-course-grid enrolled-course-grid-page">{filtered.map((enrollment) => <EnrolledCourseCard key={enrollment.id} enrollment={enrollment} />)}</div> : <EmptyState title={query ? "No matching enrolled courses" : "No courses in your learning space yet"} copy={query ? "Try a different search term." : "Choose a free course or complete checkout for a paid course to see it here."} action={<Link className="button button-primary" to="/catalog">Explore course catalog</Link>} />}</>;
}

export function StudentCourseWorkspace() {
  const { courseId } = useParams(); const id = Number(courseId); const [course, setCourse] = useState<CourseDetailResponse | null>(null); const [progress, setProgress] = useState<CourseProgressResponse | null>(null); const [lessonId, setLessonId] = useState<number | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState(""); const [saving, setSaving] = useState(false);
  const load = async () => { if (!Number.isFinite(id)) { setError("This course URL is invalid."); setLoading(false); return; } try { const [courseResult, progressResult] = await Promise.all([coursesApi.getCourseById(id), coursesApi.getCourseProgress(id)]); setCourse(courseResult); setProgress(progressResult); setLessonId((current) => current ?? courseResult.modules[0]?.lessons[0]?.id ?? null); } catch (cause) { setError(messageFor(cause, "Unable to load this course.")); } finally { setLoading(false); } };
  useEffect(() => { void load(); }, [id]);
  if (loading) return <LoadingState />; if (error || !course) return <ErrorState message={error || "Course not found."} retry={() => { setLoading(true); setError(""); void load(); }} />;
  const activeLesson = course.modules.flatMap((module) => module.lessons).find((lesson) => lesson.id === lessonId) ?? course.modules[0]?.lessons[0];
  const completed = progress?.lessonProgress.find((item) => item.lessonId === activeLesson?.id)?.completion ?? 0;
  const markComplete = async () => { if (!activeLesson) return; setSaving(true); try { await coursesApi.trackProgress({ lessonId: activeLesson.id, completion: 100, watchTime: activeLesson.duration ?? 0 }); setProgress(await coursesApi.getCourseProgress(id)); } catch (cause) { setError(messageFor(cause, "Unable to save lesson progress.")); } finally { setSaving(false); } };
  return <div className="learning-layout"><section className="lesson-stage"><div className="course-breadcrumb"><Link to="/courses">My learning</Link><ChevronRight size={15} /><span>{course.title}</span></div><div className="lesson-player"><img src={courseImage(course)} alt="" /><span className="player-duration">{activeLesson?.duration ? `${activeLesson.duration} min` : "Lesson"}</span></div><div className="lesson-heading"><div><p className="eyebrow">{activeLesson ? `Lesson ${activeLesson.order + 1}` : "Course overview"}</p><h1>{activeLesson?.title ?? "Course curriculum"}</h1><p>{activeLesson?.description || course.description}</p></div>{activeLesson && <button disabled={saving || completed >= 100} className="button button-primary" onClick={() => void markComplete()}>{saving ? "Saving…" : completed >= 100 ? "Completed" : "Mark complete"} <Check size={17} /></button>}</div><article className="lesson-copy"><h2>Course description</h2><p>{course.description || "This course does not have a description yet."}</p><p className="muted-copy">Progress is saved to your CODEAN account through the backend.</p></article></section><aside className="curriculum-panel"><div className="curriculum-head"><p className="eyebrow">Course curriculum</p><h2>{course.title}</h2><div className="progress-track"><span style={{ width: `${progress?.overallCompletionPercentage ?? 0}%` }} /></div><span>{Math.round(progress?.overallCompletionPercentage ?? 0)}% complete · {progress?.completedLessons ?? 0}/{progress?.totalLessons ?? 0} lessons</span></div>{course.modules.length ? course.modules.map((module, index) => <div className="module" key={module.id}><div className="module-title"><span>{index + 1}</span><div><strong>{module.title}</strong><small>{module.lessons.length} lessons</small></div></div>{module.lessons.map((lesson) => { const value = progress?.lessonProgress.find((item) => item.lessonId === lesson.id)?.completion ?? 0; return <button className={lesson.id === lessonId ? "lesson-link active" : "lesson-link"} onClick={() => setLessonId(lesson.id)} key={lesson.id}><span className="lesson-status">{value >= 100 ? <Check size={13} /> : lesson.order + 1}</span><span>{lesson.title}<small>{lesson.duration ? `${lesson.duration} min` : "Lesson"}</small></span>{lesson.id === lessonId && <Play size={14} fill="currentColor" />}</button>; })}</div>) : <EmptyState title="No modules yet" copy="The instructor has not added curriculum to this course." />}</aside></div>;
}

export function StudentAssessments() {
  const [exams, setExams] = useState<ExamResponse[]>([]); const [homework, setHomework] = useState<HomeworkResponse[]>([]); const [challenges, setChallenges] = useState<CodingChallengeResponse[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = async () => { setLoading(true); try { const result = await Promise.all([examsApi.list({ pageSize: 100 }), homeworkApi.list({ pageSize: 100 }), judgeApi.list()]); setExams(result[0].items); setHomework(result[1].items); setChallenges(result[2]); } catch (cause) { setError(messageFor(cause, "Unable to load assessments.")); } finally { setLoading(false); } };
  useEffect(() => { void load(); }, []); if (loading) return <LoadingState />; if (error) return <ErrorState message={error} retry={() => void load()} />;
  return <><StudentPageHeader eyebrow="Knowledge checks" title="Assessments" copy="Review the assessments currently published by your instructors." /><div className="metric-row"><Metric icon={FileCheck2} label="Exams" value={String(exams.length)} /><Metric icon={FileText} label="Homework" value={String(homework.length)} tone="green" /><Metric icon={Code2} label="Code challenges" value={String(challenges.length)} tone="amber" /></div><div className="assessment-columns"><AssessmentItems title="Exams" items={exams.map((item) => ({ id: item.id, title: item.title, detail: `${item.durationMinutes} minutes · ${item.totalMarks} marks`, href: `/exams/${item.id}`, action: "Open exam" }))} icon={FileCheck2} /><AssessmentItems title="Homework" items={homework.map((item) => ({ id: item.id, title: item.title, detail: item.dueDate ? `Due ${dateLabel(item.dueDate)}` : `${item.totalMarks} marks`, href: `/homework/${item.id}`, action: "Open task" }))} icon={BookOpen} /><AssessmentItems title="Code challenges" items={challenges.map((item) => ({ id: item.id, title: item.title, detail: `${item.difficulty} · ${item.marks} marks`, href: `/judge/${item.id}`, action: "Solve" }))} icon={Code2} /></div></>;
}

function AssessmentItems({ title, items, icon: IconComponent }: { title: string; items: { id: number; title: string; detail: string; href: string; action: string }[]; icon: Icon }) {
  return <section className="assessment-list"><div className="section-heading compact"><div><p className="eyebrow">Published</p><h2>{title}</h2></div></div>{items.length ? items.map((item) => <article key={item.id}><span className="assessment-icon"><IconComponent size={18} /></span><div><h3>{item.title}</h3><p>{item.detail}</p></div><Link to={item.href}>{item.action}<ChevronRight size={15} /></Link></article>) : <p className="muted-copy">No {title.toLowerCase()} available.</p>}</section>;
}

export function StudentSchedule() {
  const [sessions, setSessions] = useState<LiveSessionResponse[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = () => { setLoading(true); void liveApi.getUpcoming().then(setSessions).catch((cause) => setError(messageFor(cause, "Unable to load your schedule."))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []); if (loading) return <LoadingState />; if (error) return <ErrorState message={error} retry={load} />;
  return <><StudentPageHeader eyebrow="Learning calendar" title="Schedule" copy="Upcoming live sessions from your instructors." />{sessions.length ? <div className="schedule-list">{sessions.map((session) => <article className="schedule-event event-blue" key={session.id}><span>{dateTimeLabel(session.startTime)}{session.endTime ? ` – ${new Date(session.endTime).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}` : ""}</span><h2>{session.title}</h2><p><UserRound size={14} /> Instructor session · {session.providerName || "Online"}</p><div className="button-row"><button className="button button-outline" onClick={() => window.open(session.meetingLink, "_blank", "noopener,noreferrer")} disabled={!session.meetingLink}>Join session <ArrowRight size={15} /></button><Link className="button button-light" to={`/live/${session.id}`}>Details</Link></div></article>)}</div> : <EmptyState title="No upcoming sessions" copy="Your instructors have not scheduled an upcoming live session." />}</>;
}

export function StudentAnnouncements() {
  const [items, setItems] = useState<AnnouncementResponse[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = () => { setLoading(true); void announcementsApi.list({ pageSize: 100 }).then((result) => setItems(result.items)).catch((cause) => setError(messageFor(cause, "Unable to load announcements."))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []); if (loading) return <LoadingState />; if (error) return <ErrorState message={error} retry={load} />;
  return <><StudentPageHeader eyebrow="Course communication" title="Announcements" copy="Updates published by your instructors and learning team." />{items.length ? <section className="announcement-feed">{items.map((item) => <article key={item.id}>{item.isPinned && <span className="pin-label">Pinned</span>}<div><span className="avatar">{item.teacherId ? "IN" : "CO"}</span><p><strong>Instructor update</strong><small>{dateLabel(item.publishedAt)}</small></p></div><h2>{item.title}</h2><p>{item.body}</p></article>)}</section> : <EmptyState title="No announcements yet" copy="Published course updates will appear here." />}</>;
}

export function StudentNotifications() {
  const [items, setItems] = useState<{ id: number; title: string; body: string; type: string; isRead: boolean; createdAt: string }[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = () => { setLoading(true); void notificationsApi.getAll({ pageSize: 100 }).then((result) => setItems(result.items)).catch((cause) => setError(messageFor(cause, "Unable to load notifications."))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []); if (loading) return <LoadingState />; if (error) return <ErrorState message={error} retry={load} />;
  const markAll = async () => { await notificationsApi.markAllAsRead(); load(); };
  return <><StudentPageHeader eyebrow="Your updates" title="Notifications" copy="Account, course, and assessment activity from the backend." action={items.some((item) => !item.isRead) ? <button className="button button-outline" onClick={() => void markAll()}>Mark all read</button> : undefined} />{items.length ? <section className="notification-list">{items.map((item) => <article className={item.isRead ? "notification-item" : "notification-item unread"} key={item.id}><span className="notification-icon"><MessageSquareText size={18} /></span><div><h2>{item.title}</h2><p>{item.body}</p><small>{dateTimeLabel(item.createdAt)} · {item.type}</small></div>{!item.isRead && <button className="button button-outline" onClick={() => void notificationsApi.markAsRead(item.id).then(load)}>Mark read</button>}</article>)}</section> : <EmptyState title="You are all caught up" copy="New notifications will appear here when there is activity on your account." />}</>;
}

export function StudentProfile() {
  const { user } = useUser();
  const { openUserProfile } = useClerk();
  const [profile, setProfile] = useState<{ fullName: string; email: string; phone?: string; grade?: string; school?: string } | null>(null); const [identity, setIdentity] = useState<{ role: string; emailConfirmed: boolean } | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = async () => { setLoading(true); try { const [profileResult, identityResult] = await Promise.all([usersApi.getStudentProfile(), authApi.getCurrentUser()]); setProfile(profileResult); setIdentity({ role: identityResult.role, emailConfirmed: identityResult.emailConfirmed }); } catch (cause) { setError(messageFor(cause, "Unable to load your profile.")); } finally { setLoading(false); } };
  useEffect(() => { load(); }, []); if (loading) return <LoadingState />; if (error || !profile) return <ErrorState message={error || "Profile not found."} retry={load} />;
  const initials = profile.fullName.split(" ").map((part) => part[0]).join("").slice(0, 2).toUpperCase() || "U";
  const profileValues = [profile.fullName, profile.email, profile.phone, profile.grade, profile.school];
  const profileCompletion = Math.round((profileValues.filter(Boolean).length / profileValues.length) * 100);
  const emailStatus = identity?.emailConfirmed ? "Verified" : "Needs attention";
  const profileComplete = profileCompletion >= 100;
  return <><StudentPageHeader eyebrow="Your account" title="Profile" copy="Manage your identity, learning details, and account connections." action={<Link className="button button-primary" to="/settings"><Pencil size={15} /> Edit profile</Link>} /><section className="profile-page"><article className="profile-cover"><div className="profile-cover-glow" /><div className="profile-hero-content"><button className="profile-avatar profile-avatar-large" type="button" onClick={() => openUserProfile()} aria-label="Change profile photo">{user?.imageUrl ? <img src={user.imageUrl} alt="" /> : initials}<span className="profile-avatar-camera"><Camera size={14} /></span></button><div className="profile-identity"><p className="profile-overline">CODEAN learner</p><h2>{profile.fullName}</h2><p>{profile.email}</p><div className="profile-badges"><span className="profile-role-badge"><GraduationCap size={14} /> {identity?.role || "Student"}</span><span className={identity?.emailConfirmed ? "profile-verified" : "profile-pending"}><ShieldCheck size={14} /> {emailStatus}</span></div></div><div className="profile-hero-actions"><button className="button button-light" type="button" onClick={() => openUserProfile()}><Camera size={15} /> Change photo</button><Link className="profile-text-link" to="/settings">Account settings <ArrowRight size={15} /></Link></div></div></article><div className="profile-content-grid"><div className="profile-main-column"><section className="profile-panel profile-overview-panel"><div className="profile-panel-heading"><div><p className="eyebrow">At a glance</p><h2>Your account overview</h2></div><span className="profile-live-indicator"><span /> Live account</span></div><div className="profile-stat-grid"><div><span className="profile-stat-icon blue"><ShieldCheck size={17} /></span><p><strong>{profileCompletion}%</strong><small>Profile complete</small></p></div><div><span className="profile-stat-icon green"><BookOpen size={17} /></span><p><strong>{identity?.role || "Student"}</strong><small>Current role</small></p></div><div><span className="profile-stat-icon amber"><Mail size={17} /></span><p><strong>{emailStatus}</strong><small>Email status</small></p></div></div></section><section className="profile-panel"><div className="profile-panel-heading"><div><p className="eyebrow">Personal information</p><h2>Profile details</h2></div><Link className="profile-panel-link" to="/settings">Edit details <ChevronRight size={15} /></Link></div><div className="profile-details-grid"><div className="profile-detail-item"><span><Mail size={17} /></span><div><small>Email address</small><strong>{profile.email}</strong></div></div><div className="profile-detail-item"><span><Phone size={17} /></span><div><small>Phone number</small><strong>{profile.phone || "Not provided"}</strong></div></div><div className="profile-detail-item"><span><GraduationCap size={17} /></span><div><small>Grade</small><strong>{profile.grade || "Not provided"}</strong></div></div><div className="profile-detail-item"><span><School size={17} /></span><div><small>School</small><strong>{profile.school || "Not provided"}</strong></div></div></div></section></div><aside className="profile-side-column"><section className={`profile-panel profile-completion-panel ${profileComplete ? "profile-complete-panel" : ""}`}><div className="profile-panel-heading"><div><p className="eyebrow">Profile strength</p><h2>{profileComplete ? "Profile complete" : "Keep it complete"}</h2></div><span className="profile-completion-value">{profileCompletion}%</span></div><div className="profile-progress-track"><span style={{ width: `${profileCompletion}%` }} /></div><p>{profileComplete ? "Your account details are complete. You can update them any time from Settings." : "Add your grade, school, and phone number to help CODEAN personalize your learning experience."}</p><Link className="button button-primary" to="/settings">{profileComplete ? "Review settings" : "Complete profile"} <ArrowRight size={15} /></Link></section><section className="profile-panel profile-security-panel"><div className="profile-panel-heading"><div><p className="eyebrow">Security & privacy</p><h2>Connected services</h2></div><ShieldCheck size={19} /></div><div className="profile-connection"><span className="connection-icon clerk">C</span><div><strong>Clerk authentication</strong><small>Secure sign-in and profile photo</small></div><span className="connection-status">Connected</span></div><div className="profile-connection"><span className="connection-icon database">S</span><div><strong>Supabase profile</strong><small>Learning data and preferences</small></div><span className="connection-status">Synced</span></div></section></aside></div></section></>;
}

export function StudentSettings() {
  const { user } = useUser();
  const { openUserProfile } = useClerk();
  const [profile, setProfile] = useState<{ fullName: string; email: string; phone?: string; grade?: string; school?: string } | null>(null);
  const [identity, setIdentity] = useState<CurrentUserResponse | null>(null);
  const [draft, setDraft] = useState({ fullName: "", phone: "", grade: "", school: "" });
  const [saved, setSaved] = useState(false);
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const [profileResult, identityResult] = await Promise.all([usersApi.getStudentProfile(), authApi.getCurrentUser()]);
      setProfile(profileResult);
      setIdentity(identityResult);
      setDraft({ fullName: profileResult.fullName, phone: profileResult.phone || "", grade: profileResult.grade || "", school: profileResult.school || "" });
    } catch (cause) {
      setError(messageFor(cause, "Unable to load your account settings."));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void load(); }, []);

  const validate = () => {
    const next: Record<string, string> = {};
    if (draft.fullName.length < 2) next.fullName = "Enter your full name.";
    if (draft.fullName.length > 120) next.fullName = "Your name must be 120 characters or fewer.";
    if (draft.phone && !/^[+()\d\s.-]{7,30}$/.test(draft.phone)) next.phone = "Enter a valid phone number or leave this blank.";
    if (draft.grade.length > 100) next.grade = "Grade must be 100 characters or fewer.";
    if (draft.school.length > 200) next.school = "School must be 200 characters or fewer.";
    setFieldErrors(next);
    return Object.keys(next).length === 0;
  };

  const save = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSaved(false);
    setError("");
    if (!validate()) return;
    setSaving(true);
    try {
      const result = await usersApi.updateStudentProfile(draft);
      setProfile(result);
      setDraft({ fullName: result.fullName, phone: result.phone || "", grade: result.grade || "", school: result.school || "" });
      let clerkSyncWarning = "";
      if (user) {
        try {
          const [firstName, ...rest] = result.fullName.split(/\s+/);
          await user.update({ firstName, lastName: rest.join(" ") });
          await user.reload();
        } catch {
          clerkSyncWarning = "Your CODEAN profile was saved. Display-name sync with Clerk will finish on your next session.";
        }
      }
      setSaved(true);
      setError(clerkSyncWarning);
    } catch (cause) {
      setError(messageFor(cause, "We could not save your changes. Please try again."));
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <LoadingState label="Loading your account settings..." />;
  if (!profile || !identity) return <ErrorState message={error || "Settings are unavailable."} retry={load} />;
  const initials = profile.fullName.split(" ").map((part) => part[0]).join("").slice(0, 2).toUpperCase() || "U";
  const emailStatus = identity.emailConfirmed ? "Verified" : "Needs attention";
  return <><StudentPageHeader eyebrow="Account settings" title="Settings" copy="Manage the information connected to your CODEAN account." action={<Link className="button button-outline" to="/profile"><ArrowLeft size={15} /> Back to profile</Link>} />{(error || saved) && <div className={`auth-notice ${saved ? "warning" : "error"}`} role="status"><span className="auth-notice-icon">{saved ? "✓" : "!"}</span><div><strong>{saved && !error ? "Profile saved" : saved ? "Profile saved with a note" : "Could not save changes"}</strong><p>{error || "Your latest details are now stored in your CODEAN account."}</p></div></div>}<section className="settings-page"><aside className="settings-sidebar"><div className="settings-account-mini">{user?.imageUrl ? <img src={user.imageUrl} alt="" /> : <span>{initials}</span>}<div><strong>{profile.fullName}</strong><small>{profile.email}</small></div></div><nav aria-label="Settings sections"><a className="active" href="#profile-details">Profile details</a><a href="#sign-in-security">Sign-in & security</a></nav><p className="settings-sidebar-note">Your profile data is saved through the CODEAN API and stored in Supabase.</p></aside><div className="settings-content"><form className="settings-form-card" onSubmit={(event) => void save(event)} noValidate><header className="settings-form-header"><div><p className="eyebrow">Personal information</p><h2>Profile details</h2><p>Keep your identity and learning details accurate so your workspace stays personalized.</p></div><span className="settings-status-badge"><span /> Live account</span></header><div className="settings-form-grid" id="profile-details"><label className="settings-field settings-field-wide"><span>Full name</span><input value={draft.fullName} onChange={(event) => setDraft({ ...draft, fullName: event.target.value.trimStart() })} aria-invalid={Boolean(fieldErrors.fullName)} />{fieldErrors.fullName && <small className="settings-field-error">{fieldErrors.fullName}</small>}</label><label className="settings-field settings-field-wide"><span>Email address</span><div className="settings-readonly"><Mail size={16} /><input value={profile.email} readOnly aria-describedby="email-status" /><span id="email-status" className={identity.emailConfirmed ? "settings-verified" : "settings-pending"}>{emailStatus}</span></div><small className="settings-help">Email is managed by Clerk and cannot be changed from this form.</small></label><label className="settings-field"><span>Phone number <em>Optional</em></span><input value={draft.phone} onChange={(event) => setDraft({ ...draft, phone: event.target.value })} placeholder="+20 100 000 0000" aria-invalid={Boolean(fieldErrors.phone)} />{fieldErrors.phone && <small className="settings-field-error">{fieldErrors.phone}</small>}</label><label className="settings-field"><span>Grade <em>Optional</em></span><input value={draft.grade} onChange={(event) => setDraft({ ...draft, grade: event.target.value })} placeholder="e.g. Grade 10" aria-invalid={Boolean(fieldErrors.grade)} />{fieldErrors.grade && <small className="settings-field-error">{fieldErrors.grade}</small>}</label><label className="settings-field settings-field-wide"><span>School <em>Optional</em></span><input value={draft.school} onChange={(event) => setDraft({ ...draft, school: event.target.value })} placeholder="Your school or institution" aria-invalid={Boolean(fieldErrors.school)} />{fieldErrors.school && <small className="settings-field-error">{fieldErrors.school}</small>}</label></div><footer className="settings-form-actions"><Link className="button button-light" to="/profile">Cancel</Link><button className="button button-primary" type="submit" disabled={saving}>{saving ? <><span className="button-spinner" /> Saving...</> : <><Check size={15} /> Save changes</>}</button></footer></form><section className="settings-form-card settings-security-card" id="sign-in-security"><header className="settings-form-header"><div><p className="eyebrow">Security & privacy</p><h2>Connected services</h2><p>Authentication and platform data are kept in the services configured for your account.</p></div><ShieldCheck size={20} /></header><div className="settings-security-list"><div className="settings-security-row"><span className="connection-icon clerk">C</span><div><strong>Clerk authentication</strong><small>Sign-in, email verification, and profile photo</small></div><span className="connection-status">Connected</span><button className="button button-outline" type="button" onClick={() => openUserProfile()}>Manage</button></div><div className="settings-security-row"><span className="connection-icon database">S</span><div><strong>Supabase profile storage</strong><small>Learning profile and platform records</small></div><span className="connection-status">Synced</span></div></div></section></div></section></>;
}

export function StudentBilling() {
  const [subscription, setSubscription] = useState<{ planName: string; status: string; startDate: string; endDate: string }[]>([]); const [payments, setPayments] = useState<{ id: number; amount: number; currency: string; status: string; createdAt: string }[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  useEffect(() => { void Promise.all([paymentsApi.getMySubscription(), paymentsApi.getMyPayments({ pageSize: 50 })]).then(([sub, paid]) => { setSubscription(sub); setPayments(paid.items); }).catch((cause) => setError(messageFor(cause, "Unable to load billing information."))).finally(() => setLoading(false)); }, []);
  if (loading) return <LoadingState />; if (error) return <ErrorState message={error} />;
  return <><StudentPageHeader eyebrow="Account billing" title="Subscription" copy="Your current subscription and payment history from CODEAN." action={<Link className="button button-primary" to="/pricing">View plans</Link>} />{subscription.length ? <section className="billing-grid">{subscription.map((item, index) => <article className="current-plan" key={`${item.planName}-${index}`}><p className="eyebrow">Current plan</p><h2>{item.planName}</h2><p>{item.status}</p><p>{dateLabel(item.startDate)} – {dateLabel(item.endDate)}</p></article>)}</section> : <EmptyState title="No active subscription" copy="Your account does not have an active subscription yet." action={<Link className="button button-primary" to="/pricing">Explore plans</Link>} />}<section className="table-panel"><div className="section-heading compact"><div><p className="eyebrow">Payment history</p><h2>Recent payments</h2></div></div>{payments.length ? <table><thead><tr><th>Date</th><th>Amount</th><th>Status</th></tr></thead><tbody>{payments.map((item) => <tr key={item.id}><td>{dateLabel(item.createdAt)}</td><td>{item.amount} {item.currency}</td><td><span className="status-badge success">{item.status}</span></td></tr>)}</tbody></table> : <p className="muted-copy">No payments recorded.</p>}</section></>;
}

export function StudentCertificates() {
  return <><StudentPageHeader eyebrow="Verified outcomes" title="Certificates" copy="Certificates issued by CODEAN will appear here." /><EmptyState title="No certificates yet" copy="Complete the required course and assessment milestones to receive a certificate. The certificate service is not returning any records for this account yet." /></>;
}

export function StudentExam() {
  const { examId } = useParams(); const [exam, setExam] = useState<(ExamResponse & { questions?: unknown[] }) | null>(null); const [attempt, setAttempt] = useState<{ id: number; status: string; startedAt: string } | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  useEffect(() => { const id = Number(examId); if (!Number.isFinite(id)) { setError("This exam URL is invalid."); setLoading(false); return; } void examsApi.getById(id).then(setExam).catch((cause) => setError(messageFor(cause, "Unable to load this exam."))).finally(() => setLoading(false)); }, [examId]);
  if (loading) return <LoadingState />; if (error || !exam) return <ErrorState message={error || "Exam not found."} />;
  const start = async () => { try { setAttempt(await examsApi.startAttempt(exam.id)); } catch (cause) { setError(messageFor(cause, "Unable to start this exam.")); } };
  return <><StudentPageHeader eyebrow="Assessment" title={exam.title} copy={exam.description || "Exam details from your instructor."} /><section className="table-panel"><div className="metric-row"><Metric icon={Clock3} label="Duration" value={`${exam.durationMinutes} min`} /><Metric icon={Trophy} label="Total marks" value={String(exam.totalMarks)} tone="amber" /><Metric icon={CheckCircle2} label="Status" value={exam.isPublished ? "Published" : "Draft"} tone="green" /></div>{attempt ? <div className="empty-state"><CheckCircle2 size={30} /><h2>Attempt started</h2><p>Your attempt began at {dateTimeLabel(attempt.startedAt)}. Question delivery is not available in the current backend contract yet, so no fabricated questions are shown.</p></div> : <button className="button button-primary" onClick={() => void start()}>Start exam <ArrowRight size={16} /></button>}</section></>;
}

export function StudentHomework() {
  const { homeworkId } = useParams(); const [homework, setHomework] = useState<HomeworkResponse | null>(null); const [textAnswer, setTextAnswer] = useState(""); const [submitted, setSubmitted] = useState(false); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  useEffect(() => { const id = Number(homeworkId); if (!Number.isFinite(id)) { setError("This homework URL is invalid."); setLoading(false); return; } void homeworkApi.getById(id).then(setHomework).catch((cause) => setError(messageFor(cause, "Unable to load this homework."))).finally(() => setLoading(false)); }, [homeworkId]);
  const submit = async (event: FormEvent) => { event.preventDefault(); if (!homework) return; try { await homeworkApi.submit(homework.id, { submissionType: "Text", textAnswer }); setSubmitted(true); } catch (cause) { setError(messageFor(cause, "Unable to submit your homework.")); } };
  if (loading) return <LoadingState />; if (error || !homework) return <ErrorState message={error || "Homework not found."} />;
  return <><StudentPageHeader eyebrow="Course work" title={homework.title} copy={homework.description} /><section className="editor-form"><article className="table-panel"><p className="eyebrow">Assignment details</p><p>{homework.dueDate ? `Due ${dateTimeLabel(homework.dueDate)}` : "No due date"} · {homework.totalMarks} marks</p></article><form className="table-panel" onSubmit={submit}><label className="form-field"><span>Your answer</span><textarea required value={textAnswer} onChange={(event) => setTextAnswer(event.target.value)} rows={8} /></label>{submitted ? <p className="saved-message"><Check size={14} /> Submission received</p> : <button className="button button-primary">Submit homework <Send size={16} /></button>}</form></section></>;
}

export function StudentJudge() {
  const [items, setItems] = useState<CodingChallengeResponse[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = () => { setLoading(true); void judgeApi.list().then(setItems).catch((cause) => setError(messageFor(cause, "Unable to load coding challenges."))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []); if (loading) return <LoadingState />; if (error) return <ErrorState message={error} retry={load} />;
  return <><StudentPageHeader eyebrow="Practice" title="Code challenges" copy="Solve challenges published by your instructors and submit code to the judge service." />{items.length ? <div className="course-grid">{items.map((item) => <Link className="catalog-card" to={`/judge/${item.id}`} key={item.id}><div><img src={fallbackImages[item.id % fallbackImages.length]} alt="" /><span>{item.difficulty}</span></div><section><p className="eyebrow">{item.language}</p><h2>{item.title}</h2><p>{item.description}</p><strong>{item.marks} marks</strong></section></Link>)}</div> : <EmptyState title="No coding challenges yet" copy="Published challenges will appear here." />}</>;
}

export function StudentJudgeChallenge() {
  const { challengeId } = useParams(); const [challenge, setChallenge] = useState<CodingChallengeResponse | null>(null); const [source, setSource] = useState(""); const [result, setResult] = useState<string | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  useEffect(() => { const id = Number(challengeId); if (!Number.isFinite(id)) { setError("This challenge URL is invalid."); setLoading(false); return; } void judgeApi.getChallengeById(id).then((item) => { setChallenge(item); setSource(item.starterCode || ""); }).catch((cause) => setError(messageFor(cause, "Unable to load this challenge."))).finally(() => setLoading(false)); }, [challengeId]);
  const submit = async () => { if (!challenge) return; try { const response = await judgeApi.submitCode({ challengeId: challenge.id, sourceCode: source, language: challenge.language }); setResult(`Submission ${response.submissionId} is ${response.status}.`); } catch (cause) { setError(messageFor(cause, "Unable to submit code.")); } };
  if (loading) return <LoadingState />; if (error || !challenge) return <ErrorState message={error || "Challenge not found."} />;
  return <><StudentPageHeader eyebrow={`${challenge.difficulty} challenge`} title={challenge.title} copy={challenge.description} /><section className="editor-form"><article className="table-panel"><p className="eyebrow">Language</p><h2>{challenge.language}</h2><p>{challenge.marks} marks</p></article><section className="table-panel"><label className="form-field"><span>Source code</span><textarea value={source} onChange={(event) => setSource(event.target.value)} rows={18} spellCheck={false} /></label><button className="button button-primary" onClick={() => void submit()}>Submit to judge <Send size={16} /></button>{result && <p className="saved-message"><Check size={14} /> {result}</p>}</section></section></>;
}

export function StudentLiveSessions() {
  return <StudentSchedule />;
}

export function StudentLiveRoom() {
  const { sessionId } = useParams(); const [session, setSession] = useState<LiveSessionResponse | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  useEffect(() => { const id = Number(sessionId); void liveApi.getUpcoming().then((items) => setSession(items.find((item) => item.id === id) ?? null)).catch((cause) => setError(messageFor(cause, "Unable to load this session."))).finally(() => setLoading(false)); }, [sessionId]);
  if (loading) return <LoadingState />; if (error || !session) return <ErrorState message={error || "This live session is no longer available."} />;
  return <><StudentPageHeader eyebrow="Live session" title={session.title} copy={`${dateTimeLabel(session.startTime)} · ${session.providerName || "Online session"}`} /><section className="table-panel"><p className="muted-copy">The live room is hosted by the configured external meeting provider. Use the real meeting link below when the session is active.</p><button className="button button-primary" onClick={() => window.open(session.meetingLink, "_blank", "noopener,noreferrer")} disabled={!session.meetingLink}>Open meeting <Video size={16} /></button></section></>;
}

export function PublicCatalogPage() {
  const [items, setItems] = useState<CourseResponse[]>([]); const [query, setQuery] = useState(""); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  useEffect(() => { void coursesApi.getCourses({ isPublished: true, pageSize: 100 }).then((result) => setItems(result.items)).catch((cause) => setError(messageFor(cause, "Unable to load the catalog."))).finally(() => setLoading(false)); }, []);
  const filtered = items.filter((item) => `${item.title} ${item.category}`.toLowerCase().includes(query.toLowerCase()));
  return <div className="public-site"><PublicNavLite /><main className="catalog-page"><StudentPageHeader eyebrow="Course catalog" title="Build your next capability" copy="Published learning paths from the CODEAN database." /><label className="filter-search"><Search size={17} /><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search courses" /></label>{loading ? <LoadingState /> : error ? <ErrorState message={error} /> : filtered.length ? <div className="catalog-grid">{filtered.map((item) => <Link className="catalog-card" key={item.id} to={`/catalog/${item.id}`}><div><img src={courseImage(item)} alt="" /><span>{item.category}</span></div><section><p className="eyebrow">{item.category}</p><h2>{item.title}</h2><p>{item.teacherName || "CODEAN instructor"}</p><strong>{item.price ? `${item.price} EGP` : "Free"}</strong></section></Link>)}</div> : <EmptyState title="No published courses yet" copy="The catalog is empty until an instructor publishes a course." />}</main></div>;
}

function PublicNavLite() {
  return <header className="public-nav"><Link to="/" className="brand public-brand"><span className="brand-mark"><Code2 size={20} /></span>CODEAN</Link><nav className="public-links"><Link to="/catalog">Courses</Link><Link to="/pricing">Pricing</Link></nav><Link className="button button-primary" to="/auth/login">Sign in</Link></header>;
}

export function PublicCatalogDetailPage() {
  const { courseId } = useParams(); const { isSignedIn } = useAuth(); const navigate = useNavigate(); const [course, setCourse] = useState<CourseDetailResponse | null>(null); const [loading, setLoading] = useState(true); const [enrolling, setEnrolling] = useState(false); const [error, setError] = useState("");
  useEffect(() => { const id = Number(courseId); void coursesApi.getCourseById(id).then(setCourse).catch((cause) => setError(messageFor(cause, "Unable to load the course."))).finally(() => setLoading(false)); }, [courseId]);
  if (loading) return <div className="public-site"><PublicNavLite /><LoadingState /></div>; if (error || !course) return <div className="public-site"><PublicNavLite /><ErrorState message={error || "Course not found."} /></div>;
  const enroll = async () => { if (!isSignedIn) { navigate(`/auth/login?returnTo=/catalog/${course.id}`); return; } setEnrolling(true); setError(""); try { if (course.price > 0) { const checkout = await learningApi.initiateCourseCheckout(course.id); window.location.assign(checkout.checkoutUrl); return; } await learningApi.enrollCourse(course.id); navigate(`/courses/${course.id}`); } catch (cause) { setError(messageFor(cause, "We could not start course access.")); } finally { setEnrolling(false); } };
  return <div className="public-site"><PublicNavLite /><main className="detail-page"><div className="course-detail-hero"><div><Link className="back-link" to="/catalog"><ArrowLeft size={15} /> Course catalog</Link><p className="eyebrow">{course.category}</p><h1>{course.title}</h1><p>{course.description}</p><div className="detail-meta"><span><UserRound size={16} /> {course.teacherName || "CODEAN instructor"}</span><span><BookOpen size={16} /> {course.modules.reduce((total, module) => total + module.lessons.length, 0)} lessons</span></div></div><img src={courseImage(course)} alt="" /></div><div className="course-detail-grid"><section><h2>Course curriculum</h2>{course.modules.length ? course.modules.map((module, index) => <article className="syllabus-module" key={module.id}><span>{index + 1}</span><div><h3>{module.title}</h3>{module.lessons.map((lesson) => <p key={lesson.id}><Play size={13} /> {lesson.title}</p>)}</div></article>) : <EmptyState title="Curriculum coming soon" copy="The instructor has not added modules yet." />}</section><aside className="enroll-panel"><p className="eyebrow">Course access</p><h2>{course.price ? `${course.price} EGP` : "Free"}</h2><p>{course.price ? "Secure checkout is required. Access is granted only after Paymob confirms payment." : "Enroll for free and your course will appear immediately in My courses."}</p>{error && <p className="enroll-error" role="alert">{error}</p>}<button className="button button-primary" type="button" onClick={() => void enroll()} disabled={enrolling}>{enrolling ? "Opening secure checkout…" : course.price ? isSignedIn ? "Continue to secure checkout" : "Sign in to enroll" : isSignedIn ? "Enroll for free" : "Sign in to enroll"}</button></aside></div></main></div>;
}
