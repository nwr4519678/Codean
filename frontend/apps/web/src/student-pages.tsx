import { FormEvent, ReactNode, useEffect, useMemo, useState } from "react";
import { useClerk, useUser } from "@clerk/clerk-react";
import { Link, useParams } from "react-router-dom";
import {
  ArrowLeft,
  ArrowRight,
  BookOpen,
  CalendarDays,
  Check,
  CheckCircle2,
  ChevronRight,
  Clock3,
  Code2,
  FileCheck2,
  FileText,
  MessageSquareText,
  Play,
  Search,
  Send,
  Sparkles,
  Trophy,
  UserRound,
  Users,
  Video,
} from "lucide-react";
import {
  announcementsApi,
  coursesApi,
  examsApi,
  homeworkApi,
  judgeApi,
  liveApi,
  notificationsApi,
  paymentsApi,
  usersApi,
} from "@platform/api";
import type {
  AnnouncementResponse,
  CourseDetailResponse,
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

function CourseCard({ course, progress = 0 }: { course: CourseResponse; progress?: number }) {
  return <Link className="course-card" to={`/courses/${course.id}`}>
    <div className="course-image"><img src={courseImage(course)} alt="" /><span>{course.category || "Course"}</span></div>
    <div className="course-card-body"><p>{course.category || "Course"}</p><h3>{course.title}</h3><span className="instructor">{course.teacherName || "CODEAN instructor"}</span>
      <div className="course-stats"><span><BookOpen size={14} /> {course.lessonCount} lessons</span><span><Clock3 size={14} /> {course.moduleCount} modules</span></div>
      <div className="progress-track"><span style={{ width: `${Math.max(0, Math.min(100, progress))}%` }} /></div>
      <div className="progress-label"><span>{progress ? `${Math.round(progress)}% complete` : "Not started"}</span><ChevronRight size={16} /></div>
    </div>
  </Link>;
}

export function StudentDashboard({ currentUser }: { currentUser: CurrentUserResponse }) {
  const [courses, setCourses] = useState<CourseResponse[]>([]);
  const [progress, setProgress] = useState<Record<number, CourseProgressResponse>>({});
  const [sessions, setSessions] = useState<LiveSessionResponse[]>([]);
  const [notifications, setNotifications] = useState<{ id: number; title: string; body: string; createdAt: string }[]>([]);
  const [state, setState] = useState<"loading" | "ready" | "error">("loading");
  const [error, setError] = useState("");

  const load = async () => {
    setState("loading");
    try {
      const [courseResult, sessionResult, notificationResult] = await Promise.all([
        coursesApi.getCourses({ isPublished: true, pageSize: 100 }),
        liveApi.getUpcoming(),
        notificationsApi.getAll({ pageSize: 5 }),
      ]);
      setCourses(courseResult.items);
      setSessions(sessionResult);
      setNotifications(notificationResult.items);
      const results = await Promise.allSettled(courseResult.items.map((course) => coursesApi.getCourseProgress(course.id)));
      setProgress(Object.fromEntries(results.flatMap((result, index) => result.status === "fulfilled" ? [[courseResult.items[index].id, result.value]] : [])));
      setState("ready");
    } catch (cause) {
      setError(messageFor(cause, "The student workspace is temporarily unavailable."));
      setState("error");
    }
  };

  useEffect(() => { void load(); }, []);
  if (state === "loading") return <LoadingState />;
  if (state === "error") return <ErrorState message={error} retry={() => void load()} />;

  const featured = courses.find((course) => (progress[course.id]?.overallCompletionPercentage ?? 0) > 0) ?? courses[0];
  const featuredProgress = featured ? progress[featured.id]?.overallCompletionPercentage ?? 0 : 0;
  const completedCourses = courses.filter((course) => (progress[course.id]?.overallCompletionPercentage ?? 0) >= 100).length;

  return <>
    <StudentPageHeader eyebrow="Student workspace" title={`Welcome, ${currentUser.fullName || "learner"}`} copy="Your learning activity comes directly from your CODEAN account." />
    <div className="metric-row"><Metric icon={BookOpen} label="Published courses" value={String(courses.length)} /><Metric icon={CheckCircle2} label="Completed courses" value={String(completedCourses)} tone="green" /><Metric icon={Video} label="Upcoming sessions" value={String(sessions.length)} tone="amber" /><Metric icon={MessageSquareText} label="Recent updates" value={String(notifications.length)} tone="coral" /></div>
    {!featured ? <EmptyState title="Your learning workspace is ready" copy="There are no published courses yet. When an instructor publishes one, it will appear here." action={<Link className="button button-primary" to="/catalog">Browse course catalog</Link>} /> : <section className="dashboard-grid">
      <div className="dashboard-main"><article className="continue-panel"><img src={courseImage(featured)} alt="" /><div className="continue-overlay" /><div className="continue-content"><span className="pill pill-light"><Play size={13} fill="currentColor" /> Continue learning</span><p>{featured.category} · {featured.lessonCount} lessons</p><h2>{featured.title}</h2><div className="continue-meta"><span><Clock3 size={15} /> {featured.moduleCount} modules</span><span>{Math.round(featuredProgress)}% complete</span></div><div className="progress-track progress-on-dark"><span style={{ width: `${featuredProgress}%` }} /></div><Link className="button button-light" to={`/courses/${featured.id}`}>{featuredProgress ? "Resume course" : "Open course"} <ArrowRight size={17} /></Link></div></article><section className="section-block"><div className="section-heading"><div><p className="eyebrow">Your learning</p><h2>Published courses</h2></div><Link to="/courses">View all <ArrowRight size={15} /></Link></div><div className="course-row">{courses.slice(0, 3).map((course) => <CourseCard key={course.id} course={course} progress={progress[course.id]?.overallCompletionPercentage ?? 0} />)}</div></section></div>
      <aside className="dashboard-rail"><section className="rail-section"><div className="section-heading compact"><div><p className="eyebrow">Coming up</p><h2>Live sessions</h2></div><Link to="/schedule"><CalendarDays size={17} /></Link></div>{sessions.length ? <div className="timeline-list">{sessions.slice(0, 3).map((session) => <div className="timeline-item" key={session.id}><time>{new Date(session.startTime).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</time><span className="timeline-dot live" /><div><strong>{session.title}</strong><span>{dateTimeLabel(session.startTime)}</span><em>{session.status}</em></div></div>)}</div> : <p className="muted-copy">No upcoming sessions scheduled.</p>}</section><section className="rail-section activity-section"><div className="section-heading compact"><div><p className="eyebrow">Recent</p><h2>Notifications</h2></div><Link to="/notifications">View all</Link></div>{notifications.length ? <div className="activity-list">{notifications.slice(0, 3).map((item) => <div className="activity-item" key={item.id}><span><Check size={14} /></span><p>{item.title}<small>{dateLabel(item.createdAt)}</small></p></div>)}</div> : <p className="muted-copy">No recent updates.</p>}</section></aside>
    </section>}
  </>;
}

export function StudentCourses() {
  const [items, setItems] = useState<CourseResponse[]>([]); const [query, setQuery] = useState(""); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = () => { setLoading(true); void coursesApi.getCourses({ isPublished: true, pageSize: 100 }).then((result) => setItems(result.items)).catch((cause) => setError(messageFor(cause, "Unable to load courses."))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []);
  const filtered = useMemo(() => items.filter((course) => `${course.title} ${course.category} ${course.description}`.toLowerCase().includes(query.toLowerCase())), [items, query]);
  return <><StudentPageHeader eyebrow="Course library" title="My learning" copy="Browse the current course catalog and open a real course from the platform." /><div className="toolbar"><label className="filter-search"><Search size={17} /><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Filter courses" /></label></div>{loading ? <LoadingState /> : error ? <ErrorState message={error} retry={load} /> : filtered.length ? <div className="course-grid">{filtered.map((course) => <CourseCard key={course.id} course={course} />)}</div> : <EmptyState title={query ? "No matching courses" : "No published courses yet"} copy={query ? "Try a different search term." : "An instructor needs to publish a course before it appears here."} />}</>;
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
  const [profile, setProfile] = useState<{ fullName: string; email: string; phone?: string; grade?: string; school?: string } | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = () => { setLoading(true); void usersApi.getStudentProfile().then(setProfile).catch((cause) => setError(messageFor(cause, "Unable to load your profile."))).finally(() => setLoading(false)); };
  useEffect(() => { load(); }, []); if (loading) return <LoadingState />; if (error || !profile) return <ErrorState message={error || "Profile not found."} retry={load} />;
  const initials = profile.fullName.split(" ").map((part) => part[0]).join("").slice(0, 2).toUpperCase() || "U";
  return <><StudentPageHeader eyebrow="Your account" title="Profile" copy="Your personal learning profile from CODEAN." action={<button className="button button-outline" type="button" onClick={() => openUserProfile()}>Manage profile photo</button>} /><section className="profile-grid"><article className="profile-card profile-card-featured"><span className="profile-avatar">{user?.imageUrl ? <img src={user.imageUrl} alt="" /> : initials}</span><div><p className="eyebrow">Your identity</p><h2>{profile.fullName}</h2><p>{profile.email}</p><span className="status-badge success">Student</span></div><p className="profile-card-note">Your photo is managed by Clerk. Google and Microsoft profile photos appear here automatically, and you can change yours from Manage profile photo.</p></article><section className="table-panel"><div className="section-heading compact"><div><p className="eyebrow">Account information</p><h2>Profile details</h2></div><Link className="button button-primary" to="/settings">Edit profile</Link></div><dl className="detail-list"><div><dt>Email</dt><dd>{profile.email}</dd></div><div><dt>Phone</dt><dd>{profile.phone || "Not provided"}</dd></div><div><dt>Grade</dt><dd>{profile.grade || "Not provided"}</dd></div><div><dt>School</dt><dd>{profile.school || "Not provided"}</dd></div></dl></section></section></>;
}

export function StudentSettings() {
  const { user } = useUser();
  const [profile, setProfile] = useState<{ fullName: string; phone?: string; grade?: string; school?: string } | null>(null); const [saved, setSaved] = useState(false); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  useEffect(() => { void usersApi.getStudentProfile().then(setProfile).catch((cause) => setError(messageFor(cause, "Unable to load settings."))).finally(() => setLoading(false)); }, []);
  const save = async (event: FormEvent<HTMLFormElement>) => { event.preventDefault(); setError(""); setSaved(false); const data = new FormData(event.currentTarget); const fullName = String(data.get("fullName") || "").trim(); if (fullName.length < 2) { setError("Please enter your full name."); return; } try { const result = await usersApi.updateStudentProfile({ fullName, phone: String(data.get("phone") || "").trim(), grade: String(data.get("grade") || "").trim(), school: String(data.get("school") || "").trim() }); let clerkSyncWarning = ""; if (user) { try { const [firstName, ...rest] = fullName.split(/\s+/); await user.update({ firstName, lastName: rest.join(" ") }); await user.reload(); } catch { clerkSyncWarning = "Your CODEAN profile was saved. Clerk could not refresh the display name yet; it will sync on your next session."; } } setProfile(result); setSaved(true); if (clerkSyncWarning) setError(clerkSyncWarning); } catch (cause) { setError(messageFor(cause, "We could not save your changes. Please try again.")); } };
  if (loading) return <LoadingState />; if (!profile) return <ErrorState message={error || "Settings are unavailable."} />;
  return <><StudentPageHeader eyebrow="Account settings" title="Settings" copy="Keep your CODEAN profile information current." />{error && <div className={`auth-notice ${saved ? "warning" : "error"}`} role="alert"><span className="auth-notice-icon">{saved ? "i" : "!"}</span><div><strong>{saved ? "Profile saved" : "Could not save changes"}</strong><p>{error}</p></div></div>}<form className="editor-form" onSubmit={save}><section><div className="form-section-heading"><p className="eyebrow">Personal details</p><p className="muted-copy">These details are stored in your CODEAN profile in Supabase.</p></div><label className="form-field"><span>Full name</span><input name="fullName" defaultValue={profile.fullName} required /></label><label className="form-field"><span>Phone</span><input name="phone" defaultValue={profile.phone || ""} /></label><div className="form-grid two"><label className="form-field"><span>Grade</span><input name="grade" defaultValue={profile.grade || ""} /></label><label className="form-field"><span>School</span><input name="school" defaultValue={profile.school || ""} /></label></div></section><aside><p className="eyebrow">Privacy</p><p className="muted-copy">Authentication is managed by Clerk. Your profile data is stored by the CODEAN backend in Supabase.</p><button className="button button-primary" type="submit">Save changes</button>{saved && <span className="saved-message"><Check size={14} /> Changes saved to your account</span>}</aside></form></>;
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
  const { courseId } = useParams(); const [course, setCourse] = useState<CourseDetailResponse | null>(null); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  useEffect(() => { const id = Number(courseId); void coursesApi.getCourseById(id).then(setCourse).catch((cause) => setError(messageFor(cause, "Unable to load the course."))).finally(() => setLoading(false)); }, [courseId]);
  if (loading) return <div className="public-site"><PublicNavLite /><LoadingState /></div>; if (error || !course) return <div className="public-site"><PublicNavLite /><ErrorState message={error || "Course not found."} /></div>;
  return <div className="public-site"><PublicNavLite /><main className="detail-page"><div className="course-detail-hero"><div><Link className="back-link" to="/catalog"><ArrowLeft size={15} /> Course catalog</Link><p className="eyebrow">{course.category}</p><h1>{course.title}</h1><p>{course.description}</p><div className="detail-meta"><span><UserRound size={16} /> {course.teacherName || "CODEAN instructor"}</span><span><BookOpen size={16} /> {course.modules.reduce((total, module) => total + module.lessons.length, 0)} lessons</span></div></div><img src={courseImage(course)} alt="" /></div><div className="course-detail-grid"><section><h2>Course curriculum</h2>{course.modules.length ? course.modules.map((module, index) => <article className="syllabus-module" key={module.id}><span>{index + 1}</span><div><h3>{module.title}</h3>{module.lessons.map((lesson) => <p key={lesson.id}><Play size={13} /> {lesson.title}</p>)}</div></article>) : <EmptyState title="Curriculum coming soon" copy="The instructor has not added modules yet." />}</section><aside className="enroll-panel"><p className="eyebrow">Course access</p><h2>{course.price ? `${course.price} EGP` : "Free"}</h2><p>Open this course after signing in to track your progress.</p><Link className="button button-primary" to="/auth/login">Sign in to continue</Link></aside></div></main></div>;
}
