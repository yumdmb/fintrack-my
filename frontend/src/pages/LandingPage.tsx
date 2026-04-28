import { Link } from 'react-router-dom';

/* ── Inline SVG icons (small, purposeful, no library dependency) ── */

function InvoiceIcon() {
    return (
        <svg width="28" height="28" viewBox="0 0 28 28" fill="none" aria-hidden="true">
            <rect x="4" y="2" width="20" height="24" rx="2.5" stroke="currentColor" strokeWidth="1.5" />
            <line x1="9" y1="8" x2="19" y2="8" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
            <line x1="9" y1="12" x2="16" y2="12" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
            <line x1="9" y1="16" x2="19" y2="16" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
            <line x1="9" y1="20" x2="14" y2="20" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
        </svg>
    );
}

function TrendIcon() {
    return (
        <svg width="28" height="28" viewBox="0 0 28 28" fill="none" aria-hidden="true">
            <polyline points="4,22 10,14 16,17 24,6" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" fill="none" />
            <circle cx="24" cy="6" r="2" fill="currentColor" />
        </svg>
    );
}

function ExportIcon() {
    return (
        <svg width="28" height="28" viewBox="0 0 28 28" fill="none" aria-hidden="true">
            <path d="M14 4v14" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
            <path d="M9 13l5 5 5-5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
            <path d="M4 20v4a2 2 0 002 2h16a2 2 0 002-2v-4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
        </svg>
    );
}

function ShieldIcon() {
    return (
        <svg width="28" height="28" viewBox="0 0 28 28" fill="none" aria-hidden="true">
            <path d="M14 3L4 7v6c0 6.627 4.477 12.268 10 14 5.523-1.732 10-7.373 10-14V7L14 3z" stroke="currentColor" strokeWidth="1.5" strokeLinejoin="round" />
            <polyline points="10,14 13,17 18,11" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" fill="none" />
        </svg>
    );
}

/* ── Mini dashboard preview (CSS-rendered, not an image) ── */

function DashboardPreview() {
    return (
        <div className="lp-preview" aria-hidden="true">
            <div className="lp-preview__chrome">
                <div className="lp-preview__dots">
                    <span /><span /><span />
                </div>
                <span className="lp-preview__title">Dashboard</span>
            </div>

            <div className="lp-preview__body">
                <div className="lp-preview__stats">
                    <div className="lp-preview__stat">
                        <span className="lp-preview__stat-label">Revenue</span>
                        <span className="lp-preview__stat-value">RM 84,200</span>
                    </div>
                    <div className="lp-preview__stat">
                        <span className="lp-preview__stat-label">Expenses</span>
                        <span className="lp-preview__stat-value">RM 31,750</span>
                    </div>
                    <div className="lp-preview__stat">
                        <span className="lp-preview__stat-label">Net</span>
                        <span className="lp-preview__stat-value lp-preview__stat-value--accent">RM 52,450</span>
                    </div>
                </div>

                <div className="lp-preview__chart">
                    <div className="lp-preview__bar-group">
                        <div className="lp-preview__bar lp-preview__bar--rev" style={{ height: '45%' }} />
                        <div className="lp-preview__bar lp-preview__bar--exp" style={{ height: '22%' }} />
                    </div>
                    <div className="lp-preview__bar-group">
                        <div className="lp-preview__bar lp-preview__bar--rev" style={{ height: '62%' }} />
                        <div className="lp-preview__bar lp-preview__bar--exp" style={{ height: '30%' }} />
                    </div>
                    <div className="lp-preview__bar-group">
                        <div className="lp-preview__bar lp-preview__bar--rev" style={{ height: '55%' }} />
                        <div className="lp-preview__bar lp-preview__bar--exp" style={{ height: '28%' }} />
                    </div>
                    <div className="lp-preview__bar-group">
                        <div className="lp-preview__bar lp-preview__bar--rev" style={{ height: '78%' }} />
                        <div className="lp-preview__bar lp-preview__bar--exp" style={{ height: '35%' }} />
                    </div>
                    <div className="lp-preview__bar-group">
                        <div className="lp-preview__bar lp-preview__bar--rev" style={{ height: '90%' }} />
                        <div className="lp-preview__bar lp-preview__bar--exp" style={{ height: '40%' }} />
                    </div>
                    <div className="lp-preview__bar-group">
                        <div className="lp-preview__bar lp-preview__bar--rev" style={{ height: '72%' }} />
                        <div className="lp-preview__bar lp-preview__bar--exp" style={{ height: '32%' }} />
                    </div>
                </div>

                <div className="lp-preview__rows">
                    <div className="lp-preview__row">
                        <span className="lp-preview__row-label">INV-0041</span>
                        <span className="lp-preview__row-status">Finalized</span>
                        <span className="lp-preview__row-amount">RM 12,800</span>
                    </div>
                    <div className="lp-preview__row">
                        <span className="lp-preview__row-label">INV-0040</span>
                        <span className="lp-preview__row-status lp-preview__row-status--draft">Draft</span>
                        <span className="lp-preview__row-amount">RM 6,340</span>
                    </div>
                    <div className="lp-preview__row">
                        <span className="lp-preview__row-label">INV-0039</span>
                        <span className="lp-preview__row-status">Finalized</span>
                        <span className="lp-preview__row-amount">RM 24,100</span>
                    </div>
                </div>
            </div>
        </div>
    );
}

/* ── Landing Page ── */

export function LandingPage() {
    return (
        <div className="lp">
            {/* ── Top bar ── */}
            <header className="lp-topbar">
                <div className="lp-topbar__inner">
                    <div className="lp-topbar__brand">
                        <span className="lp-topbar__logo">Fintrack</span>
                    </div>
                    <nav className="lp-topbar__nav" aria-label="Landing navigation">
                        <a href="#features" className="lp-topbar__link">Features</a>
                        <a href="#how-it-works" className="lp-topbar__link">How it works</a>
                        <Link to="/sign-in" className="button button--primary lp-topbar__cta">
                            Sign in
                        </Link>
                    </nav>
                </div>
            </header>

            {/* ── Hero ── */}
            <section className="lp-hero">
                <div className="lp-hero__inner">
                    <div className="lp-hero__text">
                        <p className="lp-hero__eyebrow">Malaysia-ready e-invoicing</p>
                        <h1 className="lp-hero__headline">
                            Your invoices, expenses, and financial clarity in one place.
                        </h1>
                        <p className="lp-hero__sub">
                            Built for Malaysian SMEs. Create invoices with line items and SST,
                            track every expense, finalize for LHDN compliance, export to JSON or CSV,
                            and see your monthly trends at a glance.
                        </p>
                        <div className="lp-hero__actions">
                            <Link to="/sign-in" className="button button--primary lp-hero__cta">
                                Get started
                            </Link>
                            <a href="#features" className="button button--secondary lp-hero__cta-secondary">
                                See what's included
                            </a>
                        </div>
                    </div>
                    <div className="lp-hero__visual">
                        <DashboardPreview />
                    </div>
                </div>
            </section>

            {/* ── Metric strip ── */}
            <section className="lp-metrics" aria-label="Key numbers">
                <div className="lp-metrics__inner">
                    <div className="lp-metrics__item">
                        <span className="lp-metrics__number">100%</span>
                        <span className="lp-metrics__label">Malaysia compliant</span>
                    </div>
                    <div className="lp-metrics__divider" aria-hidden="true" />
                    <div className="lp-metrics__item">
                        <span className="lp-metrics__number">SST + TIN</span>
                        <span className="lp-metrics__label">Built-in tax fields</span>
                    </div>
                    <div className="lp-metrics__divider" aria-hidden="true" />
                    <div className="lp-metrics__item">
                        <span className="lp-metrics__number">JSON + CSV</span>
                        <span className="lp-metrics__label">Export formats</span>
                    </div>
                    <div className="lp-metrics__divider" aria-hidden="true" />
                    <div className="lp-metrics__item">
                        <span className="lp-metrics__number">Real-time</span>
                        <span className="lp-metrics__label">Dashboard trends</span>
                    </div>
                </div>
            </section>

            {/* ── Features ── */}
            <section className="lp-features" id="features">
                <div className="lp-features__inner">
                    <p className="lp-section-eyebrow">What you get</p>
                    <h2 className="lp-section-heading">Everything a Malaysian SME needs to manage money, nothing it doesn't.</h2>

                    <div className="lp-feature-list">
                        <article className="lp-feature">
                            <div className="lp-feature__icon">
                                <InvoiceIcon />
                            </div>
                            <div className="lp-feature__content">
                                <h3 className="lp-feature__title">Invoice management</h3>
                                <p className="lp-feature__desc">
                                    Create invoices with multiple line items, automatic subtotal and SST calculation,
                                    customer details, and Malaysian compliance fields. Finalize when ready, edit until then.
                                </p>
                            </div>
                        </article>

                        <article className="lp-feature">
                            <div className="lp-feature__icon">
                                <TrendIcon />
                            </div>
                            <div className="lp-feature__content">
                                <h3 className="lp-feature__title">Expense tracking</h3>
                                <p className="lp-feature__desc">
                                    Log every business expense with categories, dates, and amounts.
                                    See where money goes without spreadsheet gymnastics.
                                    Company-scoped isolation keeps your data private.
                                </p>
                            </div>
                        </article>

                        <article className="lp-feature">
                            <div className="lp-feature__icon">
                                <ExportIcon />
                            </div>
                            <div className="lp-feature__content">
                                <h3 className="lp-feature__title">LHDN-ready export</h3>
                                <p className="lp-feature__desc">
                                    Finalized invoices export to JSON or CSV with all Malaysia-specific fields:
                                    TIN, SST registration, issue dates, and currency codes.
                                    Ready for e-invoice submission workflows.
                                </p>
                            </div>
                        </article>

                        <article className="lp-feature">
                            <div className="lp-feature__icon">
                                <ShieldIcon />
                            </div>
                            <div className="lp-feature__content">
                                <h3 className="lp-feature__title">Role-based access</h3>
                                <p className="lp-feature__desc">
                                    Admin, accountant, and staff roles with company scoping.
                                    JWT authentication with secure token handling.
                                    Each user sees only what they need.
                                </p>
                            </div>
                        </article>
                    </div>
                </div>
            </section>

            {/* ── How it works ── */}
            <section className="lp-steps" id="how-it-works">
                <div className="lp-steps__inner">
                    <p className="lp-section-eyebrow">How it works</p>
                    <h2 className="lp-section-heading">From sign-in to export in four steps.</h2>

                    <ol className="lp-step-list">
                        <li className="lp-step">
                            <span className="lp-step__number">01</span>
                            <div className="lp-step__content">
                                <h3 className="lp-step__title">Sign in with your company account</h3>
                                <p className="lp-step__desc">
                                    Your admin provisions accounts. Sign in with email and password.
                                    The system bootstraps your company context automatically.
                                </p>
                            </div>
                        </li>
                        <li className="lp-step">
                            <span className="lp-step__number">02</span>
                            <div className="lp-step__content">
                                <h3 className="lp-step__title">Create invoices and log expenses</h3>
                                <p className="lp-step__desc">
                                    Add line items with descriptions, quantities, and unit prices.
                                    SST calculates automatically. Expenses are categorized and dated.
                                </p>
                            </div>
                        </li>
                        <li className="lp-step">
                            <span className="lp-step__number">03</span>
                            <div className="lp-step__content">
                                <h3 className="lp-step__title">Finalize and export</h3>
                                <p className="lp-step__desc">
                                    Lock invoices for compliance, then download as JSON or CSV.
                                    All Malaysian fields are included for e-invoice submission.
                                </p>
                            </div>
                        </li>
                        <li className="lp-step">
                            <span className="lp-step__number">04</span>
                            <div className="lp-step__content">
                                <h3 className="lp-step__title">Review your financial trends</h3>
                                <p className="lp-step__desc">
                                    The dashboard shows revenue vs. expenses over time,
                                    outstanding balances, and period summaries.
                                    No accounting degree required.
                                </p>
                            </div>
                        </li>
                    </ol>
                </div>
            </section>

            {/* ── CTA band ── */}
            <section className="lp-cta-band">
                <div className="lp-cta-band__inner">
                    <h2 className="lp-cta-band__heading">Ready to take control of your finances?</h2>
                    <p className="lp-cta-band__sub">
                        Sign in to your workspace and start managing invoices and expenses today.
                    </p>
                    <Link to="/sign-in" className="button button--primary lp-cta-band__button">
                        Get started now
                    </Link>
                </div>
            </section>

            {/* ── Footer ── */}
            <footer className="lp-footer">
                <div className="lp-footer__inner">
                    <div className="lp-footer__brand">
                        <span className="lp-footer__logo">Fintrack</span>
                        <p className="lp-footer__tagline">
                            Invoice and expense management for Malaysian SMEs.
                        </p>
                    </div>
                    <div className="lp-footer__links">
                        <span className="lp-footer__link-heading">Product</span>
                        <a href="#features" className="lp-footer__link">Features</a>
                        <a href="#how-it-works" className="lp-footer__link">How it works</a>
                        <Link to="/sign-in" className="lp-footer__link">Sign in</Link>
                    </div>
                    <div className="lp-footer__links">
                        <span className="lp-footer__link-heading">Built with</span>
                        <span className="lp-footer__link lp-footer__link--static">ASP.NET Core</span>
                        <span className="lp-footer__link lp-footer__link--static">React + Vite</span>
                        <span className="lp-footer__link lp-footer__link--static">PostgreSQL</span>
                    </div>
                </div>
                <div className="lp-footer__bottom">
                    <p>A learning project. Not for production use.</p>
                </div>
            </footer>
        </div>
    );
}
