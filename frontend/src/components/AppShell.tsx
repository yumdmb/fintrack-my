import { NavLink, Outlet } from 'react-router-dom';
import { appConfig } from '../lib/config';

const navigation = [
    { to: '/dashboard', label: 'Dashboard' },
    { to: '/invoices', label: 'Invoices' },
    { to: '/expenses', label: 'Expenses' },
    { to: '/team', label: 'Team' },
];

export function AppShell() {
    return (
        <div className="shell">
            <aside className="sidebar">
                <div className="sidebar__brand">
                    <span className="sidebar__eyebrow">Bootstrap</span>
                    <h1>Fintrack</h1>
                    <p>Malaysia-ready e-invoice and expense workflows for ASP.NET Core practice.</p>
                </div>

                <nav className="nav" aria-label="Primary navigation">
                    {navigation.map((item) => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            className={({ isActive }) =>
                                isActive ? 'nav__link nav__link--active' : 'nav__link'
                            }
                        >
                            {item.label}
                        </NavLink>
                    ))}
                </nav>

                <div className="sidebar__meta">
                    <span className="sidebar__meta-label">API base URL</span>
                    <code>{appConfig.apiBaseUrl}</code>
                </div>
            </aside>

            <main className="content">
                <header className="hero">
                    <div>
                        <span className="hero__eyebrow">Workspace Shell</span>
                        <h2>Backend and frontend foundations are wired for the next feature slices.</h2>
                    </div>
                    <p>
                        Use the scaffold to add identity, invoice, expense, export, and dashboard features
                        without reworking the host or client shell first.
                    </p>
                </header>

                <Outlet />
            </main>
        </div>
    );
}
