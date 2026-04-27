import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../app/auth';
import type { UserRole } from '../lib/api';

const navigation = [
    { to: '/dashboard', label: 'Dashboard', roles: ['admin', 'accountant'] satisfies UserRole[] },
    { to: '/invoices', label: 'Invoices', roles: ['admin', 'accountant', 'staff'] satisfies UserRole[] },
    { to: '/expenses', label: 'Expenses', roles: ['admin', 'accountant', 'staff'] satisfies UserRole[] },
];

export function AppShell() {
    const auth = useAuth();

    const visibleNavigation = navigation.filter((item) =>
        item.roles.some((role) => auth.user?.roles.includes(role)));

    return (
        <div className="shell">
            <aside className="shell__sidebar">
                <div className="brand-block">
                    <div className="brand-block__name">Fintrack</div>
                    <div className="brand-block__tagline">Invoice &amp; expense management</div>
                </div>

                <nav className="nav-list" aria-label="Primary navigation">
                    {visibleNavigation.map((item) => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            className={({ isActive }) =>
                                isActive ? 'nav-item nav-item--active' : 'nav-item'
                            }
                        >
                            <span>{item.label}</span>
                        </NavLink>
                    ))}
                </nav>

                <div className="account-block">
                    <div className="account-block__company">{auth.user?.companyName}</div>
                    <div className="account-block__email">{auth.user?.email}</div>
                    <div className="account-block__roles">
                        {auth.user?.roles.map((role) => (
                            <span key={role} className="role-tag">
                                {role}
                            </span>
                        ))}
                    </div>
                    <button className="button button--ghost button--block" type="button" onClick={auth.signOut}>
                        Sign out
                    </button>
                </div>
            </aside>

            <main className="shell__content">
                <Outlet />
            </main>
        </div>
    );
}
