import type { ReactNode } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './auth';
import type { UserRole } from '../lib/api';

interface RoleBoundaryProps {
    allowedRoles: UserRole[];
    fallbackPath: string;
    children: ReactNode;
}

function LoadingRouteGate() {
    return (
        <main className="route-gate">
            <section className="route-gate__panel">
                <span className="eyebrow">Loading</span>
                <h1>Restoring your session...</h1>
                <p>Validating access and resolving workspace context.</p>
            </section>
        </main>
    );
}

export function ProtectedRoute() {
    const auth = useAuth();
    const location = useLocation();

    if (auth.status === 'booting') {
        return <LoadingRouteGate />;
    }

    if (auth.status !== 'authenticated') {
        const redirectTarget = `${location.pathname}${location.search}`;
        return <Navigate replace to="/sign-in" state={{ from: redirectTarget }} />;
    }

    return <Outlet />;
}

export function RoleBoundary({ allowedRoles, fallbackPath, children }: RoleBoundaryProps) {
    const auth = useAuth();

    if (auth.status !== 'authenticated') {
        return null;
    }

    const hasAccess = auth.user?.roles.some((role) => allowedRoles.includes(role)) ?? false;

    return hasAccess ? <>{children}</> : <Navigate replace to={fallbackPath} />;
}
