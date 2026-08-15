import { FormEvent, useEffect, useState } from "react";
import { SignIn, SignUp, useAuth, useUser } from "@clerk/clerk-react";
import {
  ArrowLeft,
  ArrowRight,
  BadgeCheck,
  Bell,
  BookOpen,
  CalendarDays,
  Check,
  CheckCircle2,
  ChevronRight,
  Clock3,
  Code2,
  Copy,
  CreditCard,
  Download,
  FileCheck2,
  FileText,
  Filter,
  GripVertical,
  KeyRound,
  LockKeyhole,
  Mail,
  Menu,
  MessageSquareText,
  MoreHorizontal,
  Pencil,
  Play,
  Plus,
  Radio,
  Search,
  Send,
  Settings,
  ShieldCheck,
  Sparkles,
  Trash2,
  TrendingUp,
  Trophy,
  UserRound,
  Users,
  Video,
  X,
} from "lucide-react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { authApi, coursesApi, modulesApi } from "@platform/api";
import { assessments, courses, curriculum } from "./data";

const images = {
  hero: "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1800&q=88",
  react: "https://images.unsplash.com/photo-1633356122544-f134324a6cee?auto=format&fit=crop&w=1200&q=85",
  python: "https://images.unsplash.com/photo-1526379095098-d400fd0bf935?auto=format&fit=crop&w=1200&q=85",
  algorithms: "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=85",
  dotnet: "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=1200&q=85",
};

type Course = (typeof courses)[number];
type IconType = typeof Users;

function PageHeader({ eyebrow, title, copy, action }: { eyebrow: string; title: string; copy: string; action?: React.ReactNode }) {
  return <div className="page-heading"><div><p className="eyebrow">{eyebrow}</p><h1>{title}</h1><p>{copy}</p></div>{action}</div>;
}

function Metric({ icon: Icon, label, value, tone = "blue", detail }: { icon: IconType; label: string; value: string; tone?: string; detail?: string }) {
  return <div className="metric-box"><span className={`stat-icon ${tone}`}><Icon size={19} /></span><div><strong>{value}</strong><span>{label}</span>{detail && <small>{detail}</small>}</div></div>;
}

function Modal({ title, onClose, children }: { title: string; onClose: () => void; children: React.ReactNode }) {
  return <div className="modal-backdrop" role="presentation" onMouseDown={onClose}><section className="modal" role="dialog" aria-modal="true" aria-label={title} onMouseDown={(event) => event.stopPropagation()}><header><h2>{title}</h2><button className="icon-button" onClick={onClose} aria-label="Close"><X size={19} /></button></header>{children}</section></div>;
}

function Field({ label, children, hint }: { label: string; children: React.ReactNode; hint?: string }) {
  return <label className="form-field"><span>{label}</span>{children}{hint && <small>{hint}</small>}</label>;
}

function PublicNav() {
  const { isLoaded, isSignedIn } = useAuth();
  const { user } = useUser();
  const [open, setOpen] = useState(false);
  const signedIn = isLoaded && isSignedIn;
  const initials = [user?.firstName, user?.lastName].filter(Boolean).map((part) => part![0]).join("").slice(0, 2).toUpperCase() || "U";
  return <header className="public-nav"><Link to="/" className="brand public-brand"><span className="brand-mark"><Code2 size={20} /></span>CODEAN</Link><nav className={open ? "public-links open" : "public-links"}><Link to="/catalog">Courses</Link><Link to="/pricing">Pricing</Link><Link to="/outcomes">Outcomes</Link><Link to="/community">Community</Link></nav><div className="public-actions">{signedIn ? <Link className="public-profile-link" to="/dashboard" aria-label="Open your dashboard" title="Open dashboard">{user?.imageUrl ? <img src={user.imageUrl} alt="" /> : <span>{initials}</span>}</Link> : <><Link className="text-link" to="/auth/login">Sign in</Link><Link className="button button-primary" to="/auth/register">Start learning</Link></>}<button className="icon-button public-menu" onClick={() => setOpen(!open)} aria-label="Toggle menu"><Menu size={20} /></button></div></header>;
}

export function PublicHome() {
  const { isLoaded, isSignedIn } = useAuth();
  const dashboardLink = isLoaded && isSignedIn;
  return <div className="public-site"><PublicNav /><section className="public-hero"><img src={images.hero} alt="Students collaborating around laptops" /><div className="public-hero-shade" /><div className="public-hero-content"><p className="hero-kicker"><Sparkles size={15} /> Structured learning for serious builders</p><h1>Learn to build software that works in the real world.</h1><p>Production-minded courses, live feedback, assessments, and coding practice in one focused learning environment.</p><div><Link className="button hero-primary" to="/catalog">Explore courses <ArrowRight size={17} /></Link><Link className="button hero-secondary" to={dashboardLink ? "/dashboard" : "/auth/register"}>{dashboardLink ? "Go to dashboard" : "Create free account"}</Link></div><dl><div><dt>18k+</dt><dd>active learners</dd></div><div><dt>94%</dt><dd>course completion</dd></div><div><dt>42</dt><dd>expert-led tracks</dd></div></dl></div></section><section className="public-section" id="outcomes"><div className="public-section-head"><p className="eyebrow">One connected platform</p><h2>From first lesson to production confidence</h2><p>Every part of the learning workflow is designed to turn understanding into repeatable engineering skill.</p></div><div className="feature-grid"><Feature icon={BookOpen} title="Guided courses" copy="Structured modules, rich resources, and measurable lesson progress." /><Feature icon={Code2} title="Practice with feedback" copy="Solve challenges in a real editor and inspect test-level execution results." /><Feature icon={Video} title="Live instruction" copy="Join workshops, reviews, and office hours with your learning cohort." /><Feature icon={FileCheck2} title="Meaningful assessment" copy="Exams and assignments that test applied understanding, not recall." /></div></section><section className="public-band" id="community"><div><p className="eyebrow">Built for momentum</p><h2>A learning system that keeps the next step clear.</h2></div><Link className="button button-light" to={dashboardLink ? "/dashboard" : "/auth/register"}>{dashboardLink ? "Go to dashboard" : "Join CODEAN"} <ArrowRight size={17} /></Link></section><footer className="public-footer"><div><strong>CODEAN</strong><p>Programming education for the work that matters.</p></div><div><Link to="/catalog">Catalog</Link><Link to="/pricing">Pricing</Link><Link to="/auth/login">Sign in</Link></div><span>© 2026 CODEAN</span></footer></div>;
}

export function OutcomesPage() {
  return <div className="public-site"><PublicNav /><main className="public-section public-info-page"><div className="public-section-head"><p className="eyebrow">Outcomes</p><h1>Build confidence you can carry into real work.</h1><p>CODEAN connects structured courses, practice, feedback, and measurable progress so learning becomes visible engineering capability.</p></div><div className="feature-grid"><Feature icon={BookOpen} title="Learn with a path" copy="Follow practical modules that make the next concept and next project clear." /><Feature icon={Code2} title="Practice deliberately" copy="Use coding challenges and guided exercises to turn concepts into repeatable habits." /><Feature icon={BadgeCheck} title="Show your progress" copy="Track completed lessons, assessments, and certificates from your real account." /><Feature icon={TrendingUp} title="Keep improving" copy="Use feedback, announcements, and live sessions to keep momentum after the first lesson." /></div><section className="public-band"><div><p className="eyebrow">Ready to start?</p><h2>Choose a course and make the next step concrete.</h2></div><Link className="button button-light" to="/catalog">Explore courses <ArrowRight size={17} /></Link></section></main></div>;
}

export function CommunityPage() {
  return <div className="public-site"><PublicNav /><main className="public-section public-info-page"><div className="public-section-head"><p className="eyebrow">Community</p><h1>Learn with people who are building too.</h1><p>Stay connected to instructors and fellow learners through course updates, live sessions, and a shared place to keep the work moving.</p></div><div className="feature-grid"><Feature icon={MessageSquareText} title="Course conversations" copy="Follow instructor announcements and keep important learning updates in one inbox." /><Feature icon={Video} title="Live learning" copy="Join workshops, code reviews, and office hours when a session is published for your account." /><Feature icon={Users} title="A focused cohort" copy="Build alongside learners who care about understanding the work, not only finishing a checklist." /><Feature icon={Bell} title="Stay informed" copy="Your notification center keeps account, course, and assessment activity easy to find." /></div><section className="public-band"><div><p className="eyebrow">Join CODEAN</p><h2>Make your learning space part of your weekly routine.</h2></div><Link className="button button-light" to="/auth/register">Create your account <ArrowRight size={17} /></Link></section></main></div>;
}

function Feature({ icon: Icon, title, copy }: { icon: IconType; title: string; copy: string }) {
  return <article className="feature-item"><span><Icon size={22} /></span><h3>{title}</h3><p>{copy}</p></article>;
}

function PublicCourseCard({ course }: { course: Course }) {
  return <Link className="catalog-card" to={`/catalog/${course.id}`}><div><img src={images[course.id as keyof typeof images]} alt="" /><span>{course.level}</span></div><section><p className="eyebrow">{course.category}</p><h2>{course.title}</h2><p>{course.instructor}</p><div><span><BookOpen size={14} /> {course.lessons} lessons</span><span><Clock3 size={14} /> {course.duration}</span></div><strong>{course.id === "python" ? "Free" : "EGP 1,250"}</strong></section></Link>;
}

export function CourseCatalog() {
  const [query, setQuery] = useState("");
  const [category, setCategory] = useState("All");
  const categories = ["All", ...new Set(courses.map((course) => course.category))];
  const filtered = courses.filter((course) => (category === "All" || course.category === category) && course.title.toLowerCase().includes(query.toLowerCase()));
  return <div className="public-site"><PublicNav /><main className="catalog-page"><PageHeader eyebrow="Course catalog" title="Build your next capability" copy="Focused learning paths designed around practical engineering outcomes." /><div className="catalog-toolbar"><label className="filter-search"><Search size={17} /><input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search courses" /></label><div className="chip-row">{categories.map((item) => <button className={category === item ? "active" : ""} onClick={() => setCategory(item)} key={item}>{item}</button>)}</div></div><div className="catalog-grid">{filtered.map((course) => <PublicCourseCard course={course} key={course.id} />)}</div></main></div>;
}

export function CourseDetail() {
  const { courseId } = useParams();
  const course = courses.find((item) => item.id === courseId) ?? courses[0];
  return <div className="public-site"><PublicNav /><main className="detail-page"><div className="course-detail-hero"><div><Link className="back-link" to="/catalog"><ArrowLeft size={15} /> Course catalog</Link><p className="eyebrow">{course.category}</p><h1>{course.title}</h1><p>Master the concepts, production patterns, and engineering decisions that make this skill useful on real teams.</p><div className="detail-meta"><span><UserRound size={16} /> {course.instructor}</span><span><Clock3 size={16} /> {course.duration}</span><span><BookOpen size={16} /> {course.lessons} lessons</span></div></div><img src={images[course.id as keyof typeof images]} alt="Course technology workspace" /></div><div className="course-detail-grid"><section><h2>Course curriculum</h2>{curriculum.map((module, index) => <article className="syllabus-module" key={module.title}><span>{index + 1}</span><div><h3>{module.title}</h3>{module.lessons.map((lesson) => <p key={lesson}><Play size={13} /> {lesson}</p>)}</div></article>)}</section><aside className="enroll-panel"><p className="eyebrow">Full course access</p><h2>EGP 1,250</h2><p>Lifetime access to lessons, resources, assessments, and course updates.</p><ul><li><Check size={15} /> Expert-led curriculum</li><li><Check size={15} /> Practical exercises</li><li><Check size={15} /> Completion certificate</li></ul><Link className="button button-primary" to="/checkout?plan=career">Enroll now</Link></aside></div></main></div>;
}

const plans = [{ id: "starter", name: "Starter", price: "0", copy: "Explore foundations and selected community content.", features: ["Free foundation courses", "Community access", "Basic progress tracking"] }, { id: "career", name: "Career", price: "1,250", copy: "Complete access for consistent individual learning.", features: ["All courses and assessments", "Coding challenge library", "Certificates and live sessions"] }, { id: "pro", name: "Pro", price: "2,100", copy: "Accelerated support for ambitious outcomes.", features: ["Everything in Career", "Priority code reviews", "Monthly mentor sessions"] }];

export function PricingPage() {
  return <div className="public-site"><PublicNav /><main className="pricing-page"><PageHeader eyebrow="Simple pricing" title="Choose the support your goal needs" copy="Change or cancel your subscription without losing completed course history." /><div className="pricing-grid">{plans.map((plan, index) => <article className={index === 1 ? "price-card featured" : "price-card"} key={plan.id}>{index === 1 && <span className="popular">Most popular</span>}<p className="eyebrow">{plan.name}</p><h2><small>EGP</small>{plan.price}<span>/ month</span></h2><p>{plan.copy}</p><ul>{plan.features.map((feature) => <li key={feature}><Check size={15} /> {feature}</li>)}</ul><Link className={index === 1 ? "button button-primary" : "button button-outline"} to={plan.id === "starter" ? "/auth/register" : `/checkout?plan=${plan.id}`}>{plan.id === "starter" ? "Create account" : "Choose plan"}</Link></article>)}</div></main></div>;
}

export function AuthPage() {
  const { mode = "login" } = useParams();
  const isRegister = mode === "register";
  return <div className="auth-page"><aside><Link to="/" className="brand"><span className="brand-mark"><Code2 size={20} /></span>CODEAN</Link><div><p className="eyebrow">Focused learning</p><h2>Small steps. Real projects. Visible progress.</h2><p>Join a workspace built around doing the work, reviewing feedback, and improving deliberately.</p></div><span>Trusted by 18,000+ learners</span></aside><main><div className="auth-card clerk-auth-card"><Link className="back-link" to="/"><ArrowLeft size={15} /> Back to CODEAN</Link><p className="eyebrow">Account access</p>{isRegister ? <SignUp routing="path" path="/auth/register" signInUrl="/auth/login" afterSignUpUrl="/dashboard" appearance={{ elements: { rootBox: "clerk-root", card: "clerk-card" } }} /> : <SignIn routing="path" path="/auth/login" signUpUrl="/auth/register" afterSignInUrl="/dashboard" appearance={{ elements: { rootBox: "clerk-root", card: "clerk-card" } }} />}{isRegister && <p className="registration-note"><ShieldCheck size={14} /> Teacher accounts are created by a platform administrator.</p>}</div></main></div>;
}

export function CheckoutPage() {
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const plan = plans.find((item) => item.id === params.get("plan")) ?? plans[1];
  return <div className="checkout-page"><PublicNav /><main><Link className="back-link" to="/pricing"><ArrowLeft size={15} /> Pricing</Link><PageHeader eyebrow="Secure checkout" title="Complete your subscription" copy="Your access starts immediately after payment confirmation." /><div className="checkout-grid"><form onSubmit={(e) => { e.preventDefault(); navigate("/checkout/success"); }} className="checkout-form"><h2>Billing information</h2><div className="form-grid two"><Field label="First name"><input required /></Field><Field label="Last name"><input required /></Field></div><Field label="Email"><input required type="email" /></Field><Field label="Payment method"><div className="payment-choice"><CreditCard size={19} /><div><strong>Card or mobile wallet</strong><span>Processed securely by Paymob</span></div><CheckCircle2 size={18} /></div></Field><button className="button button-primary auth-submit">Continue to secure payment <LockKeyhole size={16} /></button></form><aside className="order-summary"><p className="eyebrow">Order summary</p><h2>{plan.name} plan</h2><p>{plan.copy}</p><div><span>Monthly subscription</span><strong>EGP {plan.price}</strong></div><div><span>Taxes</span><strong>Included</strong></div><div className="order-total"><span>Total today</span><strong>EGP {plan.price}</strong></div><p><ShieldCheck size={16} /> Secure payment. Cancel any time.</p></aside></div></main></div>;
}

export function StatusPage({ status }: { status: "success" | "unauthorized" | "forbidden" | "maintenance" | "email" | "verified" }) {
  const content = { success: [CheckCircle2, "Payment confirmed", "Your subscription is active and your learning workspace is ready."], unauthorized: [KeyRound, "Sign in required", "This page belongs to an authenticated workspace."], forbidden: [ShieldCheck, "Access restricted", "Your current role does not have permission to open this page."], maintenance: [Settings, "Brief maintenance", "We are applying an update and will be back shortly."], email: [Mail, "Check your inbox", "A secure reset link has been sent if the account exists."], verified: [BadgeCheck, "Account ready", "Your request was completed successfully."] } as const;
  const [Icon, title, copy] = content[status];
  return <main className="status-page"><Link to="/" className="brand public-brand"><span className="brand-mark"><Code2 size={20} /></span>CODEAN</Link><section><span><Icon size={30} /></span><p className="eyebrow">CODEAN workspace</p><h1>{title}</h1><p>{copy}</p><Link className="button button-primary" to={status === "success" || status === "verified" ? "/dashboard" : status === "unauthorized" ? "/auth/login" : "/"}>Continue <ArrowRight size={16} /></Link></section></main>;
}

export function AnnouncementsPage() {
  const posts = [{ title: "Live review moved to Thursday", body: "The React architecture review will begin at 6:00 PM. Your existing room link remains valid.", date: "Today", pinned: true }, { title: "New algorithms practice set", body: "Four interval and graph challenges were added to the advanced practice track.", date: "Yesterday", pinned: false }, { title: "Certificate requirements updated", body: "Complete the final assessment and 90% of course lessons to unlock your certificate.", date: "Aug 8", pinned: false }];
  return <><PageHeader eyebrow="Course communication" title="Announcements" copy="Updates from your instructors and learning team." /><div className="feed-layout"><section className="announcement-feed">{posts.map((post) => <article key={post.title}>{post.pinned && <span className="pin-label">Pinned</span>}<div><span className="avatar">MH</span><p><strong>Maya Hassan</strong><small>{post.date}</small></p></div><h2>{post.title}</h2><p>{post.body}</p><button><MessageSquareText size={15} /> Open discussion</button></article>)}</section><aside className="feed-aside"><h2>Filters</h2><label><input type="checkbox" defaultChecked /> Course updates</label><label><input type="checkbox" defaultChecked /> Live sessions</label><label><input type="checkbox" /> Platform news</label></aside></div></>;
}

export function BillingPage() {
  return <><PageHeader eyebrow="Account billing" title="Subscription" copy="Manage your active plan and review recent payments." action={<Link className="button button-primary" to="/pricing">Change plan</Link>} /><div className="billing-grid"><section className="current-plan"><div><p className="eyebrow">Current plan</p><h2>Career</h2><span>Renews September 12, 2026</span></div><strong>EGP 1,250 <small>/ month</small></strong><div className="progress-track"><span style={{ width: "68%" }} /></div><p>21 days remain in the current billing period.</p><button className="button button-outline">Manage subscription</button></section><section className="billing-method"><CreditCard size={22} /><h2>Payment method</h2><p>Managed securely through Paymob. Card details are never stored by CODEAN.</p><button className="button button-outline">Open payment portal</button></section></div><section className="table-panel"><div className="section-heading compact"><div><p className="eyebrow">Payment history</p><h2>Recent invoices</h2></div></div><table><thead><tr><th>Date</th><th>Description</th><th>Amount</th><th>Status</th><th /></tr></thead><tbody>{["Aug 12, 2026", "Jul 12, 2026", "Jun 12, 2026"].map((date) => <tr key={date}><td>{date}</td><td>Career monthly plan</td><td>EGP 1,250</td><td><span className="status-badge success">Paid</span></td><td><button className="icon-button"><Download size={16} /></button></td></tr>)}</tbody></table></section></>;
}

export function CertificatesPage() {
  const [selected, setSelected] = useState<Course | null>(null);
  return <><PageHeader eyebrow="Verified outcomes" title="Certificates" copy="Certificates earned by completing course and assessment requirements." /><div className="certificate-grid">{courses.slice(0, 3).map((course, index) => <article className="certificate-card" key={course.id}><div><span><Trophy size={24} /></span><p className="eyebrow">Certificate of completion</p><h2>{course.title}</h2><p>Issued to Nadia Hassan</p></div><footer><span>{index === 0 ? "Issued Aug 7, 2026" : "Requirements complete"}</span><button onClick={() => setSelected(course)}>View certificate <ChevronRight size={15} /></button></footer></article>)}</div>{selected && <Modal title="Certificate of completion" onClose={() => setSelected(null)}><div className="certificate-modal"><Trophy size={44} /><p className="eyebrow">CODEAN Academy</p><h2>{selected.title}</h2><p>This certifies that <strong>Nadia Hassan</strong> completed the course requirements and final assessment.</p><small>Credential CO-{selected.id.toUpperCase()}-2026-0812</small><button className="button button-primary" onClick={() => window.print()}><Download size={16} /> Print certificate</button></div></Modal>}</>;
}

export function ExamPage() {
  const { examId } = useParams();
  const [started, setStarted] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [question, setQuestion] = useState(0);
  const [answers, setAnswers] = useState<Record<number, string>>({});
  const questions = ["Which hook synchronizes a component with an external system?", "What makes a list key stable?", "Explain when derived state should be avoided."];
  if (!started) return <section className="exam-cover"><span><FileCheck2 size={28} /></span><p className="eyebrow">Assessment {examId}</p><h1>React Core Assessment</h1><p>18 questions · 45 minutes · Passing score 70%</p><ul><li><Check size={15} /> Your answers save as you move.</li><li><Check size={15} /> The assessment submits automatically when time expires.</li><li><Check size={15} /> You may review answered questions before submission.</li></ul><button className="button button-primary" onClick={() => setStarted(true)}>Start assessment <ArrowRight size={16} /></button></section>;
  if (submitted) return <section className="exam-cover result"><span><Trophy size={28} /></span><p className="eyebrow">Assessment complete</p><h1>88% · Passed</h1><p>You demonstrated strong command of state, events, and component architecture.</p><Link className="button button-primary" to="/assessments">Back to assessments</Link></section>;
  return <div className="exam-layout"><section className="question-panel"><header><div><p className="eyebrow">Question {question + 1} of {questions.length}</p><h1>{questions[question]}</h1></div><span><Clock3 size={16} /> 38:24</span></header>{question < 2 ? <div className="answer-options">{["useEffect", "useState", "useDeferredValue", "useId"].map((answer) => <label className={answers[question] === answer ? "selected" : ""} key={answer}><input type="radio" name={`q${question}`} checked={answers[question] === answer} onChange={() => setAnswers({ ...answers, [question]: answer })} />{answer}</label>)}</div> : <textarea className="long-answer" value={answers[question] ?? ""} onChange={(e) => setAnswers({ ...answers, [question]: e.target.value })} placeholder="Write your answer" />}<footer><button className="button button-outline" disabled={question === 0} onClick={() => setQuestion(question - 1)}>Previous</button>{question === questions.length - 1 ? <button className="button button-primary" onClick={() => setSubmitted(true)}>Submit exam</button> : <button className="button button-primary" onClick={() => setQuestion(question + 1)}>Next question</button>}</footer></section><aside className="question-nav"><p className="eyebrow">Question navigator</p><div>{questions.map((_, index) => <button className={`${index === question ? "active" : ""} ${answers[index] ? "answered" : ""}`} onClick={() => setQuestion(index)} key={index}>{index + 1}</button>)}</div><p>{Object.keys(answers).length} of {questions.length} answered</p></aside></div>;
}

export function HomeworkPage() {
  const [submitted, setSubmitted] = useState(false);
  if (submitted) return <section className="exam-cover result"><span><CheckCircle2 size={28} /></span><p className="eyebrow">Submission received</p><h1>Homework submitted</h1><p>Your instructor can now review your response and attachment.</p><Link className="button button-primary" to="/assessments">Back to assessments</Link></section>;
  return <><PageHeader eyebrow="Course assignment" title="Component architecture review" copy="Review the supplied feature and explain how you would divide state, effects, and component ownership." /><form className="homework-form" onSubmit={(e) => { e.preventDefault(); setSubmitted(true); }}><section><h2>Your response</h2><textarea required placeholder="Describe your approach, tradeoffs, and testing strategy." /><Field label="Attachment URL" hint="Optional. Add a repository, document, or file link."><input type="url" placeholder="https://" /></Field></section><aside><p className="eyebrow">Submission details</p><h2>Due tomorrow</h2><p>Maximum score: 25 marks</p><p>Expected effort: 60-90 minutes</p><button className="button button-primary">Submit homework <Send size={16} /></button></aside></form></>;
}

const challenges = [{ id: "two-sum", title: "Two Sum", difficulty: "Easy", category: "Arrays", acceptance: "68%" }, { id: "valid-parentheses", title: "Valid Parentheses", difficulty: "Easy", category: "Stacks", acceptance: "74%" }, { id: "merge-intervals", title: "Merge Intervals", difficulty: "Medium", category: "Intervals", acceptance: "52%" }, { id: "lru-cache", title: "LRU Cache", difficulty: "Hard", category: "Design", acceptance: "31%" }];

export function JudgePage() {
  const [query, setQuery] = useState("");
  return <><PageHeader eyebrow="Coding practice" title="Challenge library" copy="Solve focused programming problems and inspect execution feedback." /><div className="toolbar"><label className="filter-search"><Search size={17} /><input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search challenges" /></label><button className="button button-outline"><Filter size={16} /> Difficulty</button></div><div className="challenge-list">{challenges.filter((c) => c.title.toLowerCase().includes(query.toLowerCase())).map((challenge, index) => <Link to={`/judge/${challenge.id}`} key={challenge.id}><span className={index < 2 ? "challenge-status solved" : "challenge-status"}>{index < 2 ? <Check size={14} /> : <Code2 size={14} />}</span><div><h2>{challenge.title}</h2><p>{challenge.category} · {challenge.acceptance} acceptance</p></div><span className={`difficulty ${challenge.difficulty.toLowerCase()}`}>{challenge.difficulty}</span><strong>{100 + index * 50} XP</strong><ChevronRight size={17} /></Link>)}</div></>;
}

export function JudgeChallengePage() {
  const { challengeId } = useParams();
  const challenge = challenges.find((item) => item.id === challengeId) ?? challenges[0];
  const [language, setLanguage] = useState("TypeScript");
  const [output, setOutput] = useState("Run your code to inspect test results.");
  const [code, setCode] = useState("function twoSum(nums: number[], target: number): number[] {\n  // Write your solution\n  return [];\n}");
  return <div className="judge-workspace"><section className="problem-panel"><Link className="back-link" to="/judge"><ArrowLeft size={15} /> Challenges</Link><div className="problem-heading"><p className="eyebrow">{challenge.category}</p><h1>{challenge.title}</h1><span className={`difficulty ${challenge.difficulty.toLowerCase()}`}>{challenge.difficulty}</span></div><p>Given an array of integers and a target, return the indices of the two values whose sum equals the target.</p><h2>Example</h2><pre>Input: nums = [2,7,11,15], target = 9{"\n"}Output: [0,1]</pre><h2>Constraints</h2><ul><li>Each input has exactly one solution.</li><li>You may not use the same element twice.</li></ul></section><section className="editor-panel"><header><select value={language} onChange={(e) => setLanguage(e.target.value)}><option>TypeScript</option><option>Python</option><option>C#</option></select><div><button className="button button-outline" onClick={() => navigator.clipboard.writeText(code)}><Copy size={15} /></button><button className="button button-outline" onClick={() => setOutput("Accepted · 12 / 12 tests passed · 42 ms · 18.4 MB")}><Play size={15} /> Run</button><button className="button button-primary" onClick={() => setOutput("Submitted successfully · Accepted · +100 XP")}><Send size={15} /> Submit</button></div></header><textarea className="code-editor" value={code} onChange={(e) => setCode(e.target.value)} spellCheck={false} /><div className="terminal-output"><div><span /><span /><span /></div><pre>{output}</pre></div></section></div>;
}

export function LiveSessionsPage() {
  const sessions = [{ id: "react-review", title: "React architecture workshop", teacher: "Maya Hassan", time: "Today · 6:00 PM", live: true }, { id: "algorithms-office", title: "Algorithms office hours", teacher: "Lina Nasser", time: "Tomorrow · 3:30 PM", live: false }, { id: "api-recording", title: "Production API review", teacher: "Youssef Ali", time: "Recorded Aug 9", live: false }];
  return <><PageHeader eyebrow="Live learning" title="Sessions" copy="Join upcoming classes or revisit workshop recordings." /><div className="live-grid">{sessions.map((session) => <article key={session.id}><div className="live-visual"><Radio size={25} /><span className={session.live ? "live-now" : "recorded"}>{session.live ? "Live today" : "Upcoming"}</span></div><section><p className="eyebrow">{session.time}</p><h2>{session.title}</h2><p><UserRound size={14} /> {session.teacher}</p><Link className="button button-primary" to={`/live/${session.id}`}>{session.live ? "Join room" : "View details"}</Link></section></article>)}</div></>;
}

export function LiveRoomPage() {
  const { sessionId } = useParams();
  const [messages, setMessages] = useState(["Maya: Welcome everyone.", "Nadia: Will we cover effect cleanup?", "Maya: Yes, in the second example."]);
  const [message, setMessage] = useState("");
  return <div className="live-room"><section><div className="stream-stage"><Video size={45} /><span>Live classroom · {sessionId}</span><button><Play size={22} fill="currentColor" /></button></div><div className="stream-info"><div><p className="eyebrow">Live now</p><h1>React architecture workshop</h1><p>Maya Hassan · 124 learners attending</p></div><button className="button button-outline">Leave room</button></div></section><aside><header><h2>Class chat</h2><span>124 online</span></header><div className="chat-list">{messages.map((item, index) => <p key={`${item}-${index}`}>{item}</p>)}</div><form onSubmit={(e) => { e.preventDefault(); if (message.trim()) setMessages([...messages, `You: ${message.trim()}`]); setMessage(""); }}><input value={message} onChange={(e) => setMessage(e.target.value)} placeholder="Write a message" /><button><Send size={17} /></button></form></aside></div>;
}

export function NotificationsPage() {
  const [read, setRead] = useState<number[]>([2]);
  const notices = ["Your React assignment was graded: 23/25", "Live architecture workshop starts in one hour", "New lesson added to Algorithms and Problem Solving", "Your monthly payment was confirmed"];
  return <><PageHeader eyebrow="Inbox" title="Notifications" copy="Course activity, results, and account updates." action={<button className="button button-outline" onClick={() => setRead(notices.map((_, i) => i))}>Mark all read</button>} /><section className="notification-list">{notices.map((notice, index) => <article className={read.includes(index) ? "read" : ""} key={notice}><span><Bell size={17} /></span><div><h2>{notice}</h2><p>{index + 1} hours ago</p></div>{!read.includes(index) && <button onClick={() => setRead([...read, index])}>Mark read</button>}</article>)}</section></>;
}

export function ProfilePage() {
  return <><PageHeader eyebrow="Student profile" title="Nadia Hassan" copy="Your learning identity, outcomes, and current progress." action={<Link className="button button-outline" to="/settings"><Pencil size={15} /> Edit profile</Link>} /><section className="profile-hero"><span className="profile-avatar">NH</span><div><h2>Nadia Hassan</h2><p>nadia@example.com · Cairo, Egypt</p><span className="status-badge success">Active student</span></div><dl><div><dt>1,840</dt><dd>Total XP</dd></div><div><dt>8</dt><dd>Certificates</dd></div><div><dt>92%</dt><dd>Average score</dd></div></dl></section><div className="profile-grid"><section><p className="eyebrow">Current focus</p><h2>Frontend engineering track</h2><p>Building stronger React architecture, testing, and performance skills.</p><div className="progress-track"><span style={{ width: "68%" }} /></div></section><section><p className="eyebrow">Learning streak</p><h2>7 consecutive days</h2><p>12.5 focused learning hours completed this week.</p></section></div></>;
}

export function SettingsPage() {
  const [saved, setSaved] = useState(false);
  const [tab, setTab] = useState("Profile");
  return <><PageHeader eyebrow="Account preferences" title="Settings" copy="Manage your profile, security, and learning preferences." /><div className="settings-layout"><nav>{["Profile", "Security", "Notifications", "Sessions"].map((item) => <button className={tab === item ? "active" : ""} onClick={() => { setTab(item); setSaved(false); }} key={item}>{item}</button>)}</nav><form onSubmit={(e) => { e.preventDefault(); setSaved(true); }}><h2>{tab}</h2>{tab === "Profile" && <><div className="form-grid two"><Field label="Full name"><input defaultValue="Nadia Hassan" /></Field><Field label="Phone"><input defaultValue="010 1234 5678" /></Field></div><div className="form-grid two"><Field label="Grade"><input defaultValue="Grade 11" /></Field><Field label="School"><input defaultValue="Cairo International School" /></Field></div><Field label="Learning notes"><textarea defaultValue="Focused on frontend engineering and algorithms." /></Field></>}{tab === "Security" && <><Field label="Current password"><input type="password" /></Field><div className="form-grid two"><Field label="New password"><input type="password" /></Field><Field label="Confirm password"><input type="password" /></Field></div><div className="setting-row"><div><strong>Two-factor authentication</strong><span>Add an authenticator app for stronger account security.</span></div><button type="button" className="button button-outline">Configure</button></div></>}{tab === "Notifications" && <>{["Assignment results", "Live session reminders", "Course announcements", "Billing updates"].map((item) => <label className="toggle-row" key={item}><span>{item}</span><input type="checkbox" defaultChecked /></label>)}</>}{tab === "Sessions" && <>{["Windows · Chrome · Current session", "Android · Chrome · Cairo"].map((item, i) => <div className="setting-row" key={item}><div><strong>{item}</strong><span>{i === 0 ? "Active now" : "Last active 2 days ago"}</span></div>{i > 0 && <button type="button" className="button button-outline">Revoke</button>}</div>)}</>}<button className="button button-primary">Save changes</button>{saved && <span className="saved-message"><Check size={14} /> Changes saved</span>}</form></div></>;
}

export function TeacherDashboard() {
  return <><PageHeader eyebrow="Instructor workspace" title="Teaching overview" copy="Track course performance and keep learner work moving." action={<Link className="button button-primary" to="/teacher/courses/new"><Plus size={16} /> New course</Link>} /><div className="metric-row"><Metric icon={Users} value="1,284" label="Active students" detail="+8.2% this month" /><Metric icon={BookOpen} value="6" label="Published courses" tone="green" /><Metric icon={FileCheck2} value="38" label="Awaiting review" tone="amber" /><Metric icon={TrendingUp} value="4.8" label="Average rating" tone="coral" /></div><div className="teacher-dashboard-grid"><section className="table-panel"><div className="section-heading compact"><div><p className="eyebrow">Course performance</p><h2>Active courses</h2></div><Link to="/teacher/courses">View all</Link></div><table><thead><tr><th>Course</th><th>Students</th><th>Completion</th><th>Status</th></tr></thead><tbody>{courses.slice(0, 3).map((course, index) => <tr key={course.id}><td><Link to={`/teacher/courses/${course.id}`}>{course.title}</Link></td><td>{320 + index * 84}</td><td>{72 - index * 9}%</td><td><span className="status-badge success">Published</span></td></tr>)}</tbody></table></section><aside className="action-panel"><p className="eyebrow">Needs attention</p><h2>Today</h2><Link to="/teacher/homework"><FileCheck2 size={18} /><span><strong>18 homework submissions</strong><small>Ready for grading</small></span><ChevronRight size={16} /></Link><Link to="/teacher/live"><Video size={18} /><span><strong>Architecture workshop</strong><small>Starts at 6:00 PM</small></span><ChevronRight size={16} /></Link><Link to="/teacher/announcements"><Bell size={18} /><span><strong>Course announcement</strong><small>Draft saved yesterday</small></span><ChevronRight size={16} /></Link></aside></div></>;
}

export function TeacherCourses() {
  const [items, setItems] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => { authApi.getCurrentUser().then((user) => coursesApi.getCourses({ teacherId: user.userId, pageSize: 100 })).then((result) => setItems(result.items)).catch((cause) => setError(cause instanceof Error ? cause.message : "Unable to load courses.")); }, []);
  return <><PageHeader eyebrow="Course management" title="Courses" copy="Create, publish, and maintain your learning catalog." action={<Link className="button button-primary" to="/teacher/courses/new"><Plus size={16} /> Create course</Link>} />{error && <p className="form-error">{error}</p>}<section className="table-panel"><table><thead><tr><th>Course</th><th>Category</th><th>Students</th><th>Price</th><th>Status</th><th /></tr></thead><tbody>{items.map((course) => <tr key={course.id}><td><Link to={`/teacher/courses/${course.id}`}><strong>{course.title}</strong><small>{course.lessonCount} lessons</small></Link></td><td>{course.category}</td><td>{course.enrollmentCount ?? 0}</td><td>EGP {course.price}</td><td><span className={`status-badge ${course.isPublished ? "success" : "warning"}`}>{course.isPublished ? "Published" : "Draft"}</span></td><td><Link className="icon-button" to={`/teacher/courses/${course.id}/edit`}><Pencil size={15} /></Link></td></tr>)}</tbody></table></section></>;
}

export function TeacherCourseOverview() {
  const { id } = useParams();
  const [course, setCourse] = useState<any>(null); const [error, setError] = useState<string | null>(null);
  useEffect(() => { if (id) coursesApi.getCourseById(id).then(setCourse).catch((cause) => setError(cause instanceof Error ? cause.message : "Unable to load course.")); }, [id]);
  if (!course) return <section><p>{error ?? "Loading course…"}</p></section>;
  const togglePublish = async () => { try { if (course.isPublished) await coursesApi.archiveCourse(course.id); else await coursesApi.publishCourse(course.id); setCourse({ ...course, isPublished: !course.isPublished }); } catch (cause) { setError(cause instanceof Error ? cause.message : "Unable to update publishing status."); } };
  return <><PageHeader eyebrow="Course management" title={course.title} copy="Review curriculum, publishing state, and learner activity." action={<div className="button-row"><Link className="button button-outline" to={`/teacher/courses/${course.id}/edit`}><Pencil size={15} /> Edit</Link><Link className="button button-primary" to={`/teacher/courses/${course.id}/modules`}>Manage curriculum</Link></div>} />{error && <p className="form-error">{error}</p>}<div className="metric-row"><Metric icon={Users} value="—" label="Students" /><Metric icon={BookOpen} value={`${course.modules.reduce((total: number, module: any) => total + module.lessons.length, 0)}`} label="Lessons" tone="green" /><Metric icon={Clock3} value={`${course.modules.length} modules`} label="Curriculum" tone="amber" /><Metric icon={TrendingUp} value={course.isPublished ? "Published" : "Draft"} label="Status" tone="coral" /></div><button className="button button-outline" onClick={togglePublish}>{course.isPublished ? "Archive course" : "Publish course"}</button><section className="curriculum-overview"><div className="section-heading"><div><p className="eyebrow">Course structure</p><h2>Curriculum</h2></div></div>{course.modules.map((module: any, index: number) => <article key={module.id}><span>{index + 1}</span><div><h3>{module.title}</h3><p>{module.lessons.map((lesson: any) => lesson.title).join(" · ") || "No lessons yet"}</p></div><strong>{module.lessons.length} lessons</strong></article>)}</section></>;
}

export function TeacherCourseEditor() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [course, setCourse] = useState<any>(null); const [error, setError] = useState<string | null>(null); const [saving, setSaving] = useState(false);
  useEffect(() => { if (id) coursesApi.getCourseById(id).then(setCourse).catch((cause) => setError(cause instanceof Error ? cause.message : "Unable to load course.")); }, [id]);
  const submit = async (event: FormEvent<HTMLFormElement>) => { event.preventDefault(); setSaving(true); setError(null); const data = new FormData(event.currentTarget); const payload = { title: String(data.get("title")), description: String(data.get("description")), category: String(data.get("category")), price: Number(data.get("price")), thumbnail: String(data.get("thumbnail") || "") }; try { const saved = id ? await coursesApi.updateCourse(id, payload) : await coursesApi.createCourse(payload); navigate(`/teacher/courses/${saved.id}/modules`); } catch (cause) { setError(cause instanceof Error ? cause.message : "Unable to save course."); } finally { setSaving(false); } };
  return <><PageHeader eyebrow="Course editor" title={id ? "Edit course" : "Create a course"} copy="Define the course offer before building its modules and lessons." />{error && <p className="form-error">{error}</p>}<form className="editor-form" onSubmit={submit}><section><Field label="Course title"><input name="title" required defaultValue={course?.title} placeholder="Production React Architecture" /></Field><Field label="Description"><textarea name="description" required defaultValue={course?.description} /></Field><div className="form-grid two"><Field label="Category"><select name="category" defaultValue={course?.category ?? "Frontend"}><option>Frontend</option><option>Backend</option><option>Programming</option><option>Computer Science</option></select></Field><Field label="Price (EGP)"><input name="price" type="number" min="0" defaultValue={course?.price ?? 0} /></Field></div><Field label="Thumbnail URL"><input name="thumbnail" type="url" defaultValue={course?.thumbnail} placeholder="https://" /></Field></section><aside><p className="eyebrow">Publishing checklist</p>{["Clear course outcome", "Complete description", "Category selected", "Pricing confirmed"].map((item) => <p key={item}><CheckCircle2 size={15} /> {item}</p>)}<button disabled={saving} className="button button-primary">{saving ? "Saving…" : id ? "Save changes" : "Create and add modules"}</button></aside></form></>;
}

export function TeacherModules() {
  const { id } = useParams(); const [course, setCourse] = useState<any>(null); const [title, setTitle] = useState(""); const [error, setError] = useState<string | null>(null);
  const load = () => { if (id) coursesApi.getCourseById(id).then(setCourse).catch((cause) => setError(cause instanceof Error ? cause.message : "Unable to load curriculum.")); }; useEffect(load, [id]);
  const addModule = async () => { if (!title.trim() || !id) return; try { await modulesApi.create({ courseId: Number(id), title: title.trim(), description: "", monthNumber: (course?.modules?.length ?? 0) + 1, order: course?.modules?.length ?? 0 }); setTitle(""); load(); } catch (cause) { setError(cause instanceof Error ? cause.message : "Unable to create module."); } };
  const addLesson = async (module: any) => { const lessonTitle = window.prompt("Lesson title"); if (!lessonTitle?.trim()) return; try { await modulesApi.createLesson({ moduleId: module.id, title: lessonTitle.trim(), description: "", duration: 0, order: module.lessons.length }); load(); } catch (cause) { setError(cause instanceof Error ? cause.message : "Unable to create lesson."); } };
  return <><PageHeader eyebrow="Curriculum builder" title={course?.title ?? "Course modules"} copy="Organize modules and lessons, then publish each item when it is ready." action={<button className="button button-primary" onClick={addModule}><Plus size={16} /> Add module</button>} />{error && <p className="form-error">{error}</p>}<div className="module-builder-toolbar"><input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="New module title" /></div><div className="module-builder">{(course?.modules ?? []).map((module: any, moduleIndex: number) => <article key={module.id}><header><GripVertical size={17} /><span>{moduleIndex + 1}</span><div><h2>{module.title}</h2><p>{module.lessons.length} lessons</p></div><button className="icon-button" onClick={() => modulesApi.delete(module.id).then(load)}><Trash2 size={16} /></button></header>{module.lessons.map((lesson: any) => <div className="builder-lesson" key={lesson.id}><Play size={14} /><span>{lesson.title}</span><small>{lesson.duration ?? 0} min</small><span className={`status-badge ${lesson.isPublished ? "success" : "warning"}`}>{lesson.isPublished ? "Published" : "Draft"}</span></div>)}<button className="add-lesson" onClick={() => addLesson(module)}><Plus size={14} /> Add lesson</button></article>)}</div></>;
}

export function TeacherExams() {
  return <><PageHeader eyebrow="Assessment management" title="Exams" copy="Build, publish, and review course assessments." action={<Link className="button button-primary" to="/teacher/exams/new"><Plus size={16} /> New exam</Link>} /><div className="metric-row"><Metric icon={FileCheck2} value="7" label="Exams" /><Metric icon={Users} value="864" label="Attempts" tone="green" /><Metric icon={TrendingUp} value="78%" label="Pass rate" tone="amber" /></div><section className="table-panel"><table><thead><tr><th>Exam</th><th>Duration</th><th>Marks</th><th>Attempts</th><th>Status</th><th /></tr></thead><tbody>{assessments.exams.map((exam, index) => <tr key={exam}><td><strong>{exam}</strong></td><td>{45 + index * 15} min</td><td>{50 + index * 25}</td><td>{124 + index * 83}</td><td><span className={`status-badge ${index === 2 ? "warning" : "success"}`}>{index === 2 ? "Draft" : "Published"}</span></td><td><Link className="icon-button" to="/teacher/exams/new"><Pencil size={15} /></Link></td></tr>)}</tbody></table></section></>;
}

export function TeacherExamEditor() {
  const [questions, setQuestions] = useState([{ text: "Which hook synchronizes external systems?", type: "Multiple choice", marks: 5 }]);
  return <><PageHeader eyebrow="Exam builder" title="Create exam" copy="Define assessment settings and build a clear question sequence." action={<button className="button button-primary">Save exam</button>} /><div className="exam-builder"><section className="exam-settings"><h2>Exam settings</h2><Field label="Title"><input defaultValue="React Core Assessment" /></Field><div className="form-grid two"><Field label="Duration (minutes)"><input type="number" defaultValue={45} /></Field><Field label="Passing score"><input type="number" defaultValue={70} /></Field></div><Field label="Description"><textarea defaultValue="Assess core React concepts and production decision making." /></Field></section><section className="question-builder"><div className="section-heading compact"><div><p className="eyebrow">Question bank</p><h2>{questions.length} questions</h2></div><button className="button button-outline" onClick={() => setQuestions([...questions, { text: "New question", type: "Essay", marks: 10 }])}><Plus size={15} /> Add question</button></div>{questions.map((question, index) => <article key={index}><header><span>{index + 1}</span><select defaultValue={question.type}><option>Multiple choice</option><option>True / false</option><option>Essay</option><option>Programming</option></select><input type="number" defaultValue={question.marks} /><button className="icon-button" onClick={() => setQuestions(questions.filter((_, i) => i !== index))}><Trash2 size={15} /></button></header><textarea defaultValue={question.text} />{question.type === "Multiple choice" && <div className="choice-list">{["useEffect", "useState", "useId", "useRef"].map((choice, i) => <label key={choice}><input type="radio" name={`correct-${index}`} defaultChecked={i === 0} /><input defaultValue={choice} /></label>)}</div>}</article>)}</section></div></>;
}

export function TeacherHomework() {
  const [grading, setGrading] = useState(false);
  return <><PageHeader eyebrow="Course work" title="Homework" copy="Create assignments and review student submissions." action={<button className="button button-primary"><Plus size={16} /> New assignment</button>} /><div className="teacher-homework-grid"><section className="assignment-list">{assessments.homework.map((item, index) => <article key={item}><span><FileText size={19} /></span><div><h2>{item}</h2><p>{22 - index * 4} submissions · Due Aug {14 + index}</p></div><button onClick={() => setGrading(true)}>Review <ChevronRight size={15} /></button></article>)}</section><aside><p className="eyebrow">Submission queue</p><h2>38 awaiting review</h2><p>Oldest submission has been waiting 18 hours.</p><button className="button button-outline" onClick={() => setGrading(true)}>Start grading</button></aside></div>{grading && <Modal title="Grade submission" onClose={() => setGrading(false)}><form className="modal-form" onSubmit={(e) => { e.preventDefault(); setGrading(false); }}><div className="submission-preview"><strong>Nadia Hassan</strong><p>I would keep server state in the query layer and colocate transient interaction state with the feature component...</p></div><div className="form-grid two"><Field label="Score"><input type="number" defaultValue={23} /></Field><Field label="Maximum"><input disabled defaultValue="25" /></Field></div><Field label="Feedback"><textarea defaultValue="Strong ownership decisions. Add one example of effect cleanup." /></Field><button className="button button-primary">Save grade</button></form></Modal>}</>;
}

export function TeacherStudents() {
  const students = ["Nadia Hassan", "Omar Adel", "Salma Tarek", "Karim Mostafa", "Mariam Ali"];
  return <><PageHeader eyebrow="Learner management" title="Students" copy="Review engagement and progress across your courses." /><section className="table-panel"><div className="table-toolbar"><label className="filter-search"><Search size={16} /><input placeholder="Search students" /></label><button className="button button-outline"><Download size={15} /> Export</button></div><table><thead><tr><th>Student</th><th>Course</th><th>Progress</th><th>Average</th><th>Last active</th><th /></tr></thead><tbody>{students.map((student, index) => <tr key={student}><td><strong>{student}</strong><small>{student.toLowerCase().replace(" ", ".")}@example.com</small></td><td>{courses[index % courses.length].title}</td><td>{58 + index * 7}%</td><td>{84 + index * 2}%</td><td>{index + 1}h ago</td><td><button className="icon-button"><MoreHorizontal size={16} /></button></td></tr>)}</tbody></table></section></>;
}

export function TeacherLive() {
  const [scheduled, setScheduled] = useState(false);
  return <><PageHeader eyebrow="Live teaching" title="Live sessions" copy="Schedule workshops, office hours, and course reviews." action={<button className="button button-primary" onClick={() => setScheduled(true)}><Plus size={16} /> Schedule session</button>} /><div className="live-grid"><article><div className="live-visual"><Radio size={25} /><span className="live-now">Today</span></div><section><p className="eyebrow">6:00 PM · 75 min</p><h2>React architecture workshop</h2><p><Users size={14} /> 124 registered</p><button className="button button-primary">Open host room</button></section></article><article><div className="live-visual muted"><CalendarDays size={25} /></div><section><p className="eyebrow">Tomorrow</p><h2>Frontend office hours</h2><p><Users size={14} /> 48 registered</p><button className="button button-outline">Edit session</button></section></article></div>{scheduled && <Modal title="Schedule live session" onClose={() => setScheduled(false)}><form className="modal-form" onSubmit={(e) => { e.preventDefault(); setScheduled(false); }}><Field label="Session title"><input required /></Field><div className="form-grid two"><Field label="Start time"><input type="datetime-local" required /></Field><Field label="End time"><input type="datetime-local" required /></Field></div><Field label="Course"><select><option>Modern React Engineering</option><option>Python Foundations</option></select></Field><button className="button button-primary">Schedule session</button></form></Modal>}</>;
}

export function TeacherAnnouncements() {
  const [compose, setCompose] = useState(false);
  return <><PageHeader eyebrow="Learner communication" title="Announcements" copy="Publish updates to your course audiences." action={<button className="button button-primary" onClick={() => setCompose(true)}><Plus size={16} /> New announcement</button>} /><section className="announcement-feed teacher-feed">{["Live review moved to Thursday", "New algorithms practice set", "Final assessment opens Monday"].map((title, index) => <article key={title}><div><span className="avatar">MH</span><p><strong>Maya Hassan</strong><small>{index + 1} days ago</small></p></div><h2>{title}</h2><p>Published to {index === 1 ? "Algorithms and Problem Solving" : "Modern React Engineering"}</p><button><Pencil size={14} /> Edit announcement</button></article>)}</section>{compose && <Modal title="Create announcement" onClose={() => setCompose(false)}><form className="modal-form" onSubmit={(e) => { e.preventDefault(); setCompose(false); }}><Field label="Audience"><select><option>Modern React Engineering</option><option>All students</option></select></Field><Field label="Title"><input required /></Field><Field label="Message"><textarea required /></Field><label className="toggle-row"><span>Pin announcement</span><input type="checkbox" /></label><button className="button button-primary">Publish announcement</button></form></Modal>}</>;
}
