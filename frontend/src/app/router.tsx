import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute, RoleBoundary } from './ProtectedRoute';
import { useAuth } from './auth';
import { AppShell } from '../components/AppShell';
import { DashboardPage } from '../pages/DashboardPage';
import { ExpensesPage } from '../pages/ExpensesPage';
import { InvoicesPage } from '../pages/InvoicesPage';
import { NotFoundPage } from '../pages/NotFoundPage';
import { SignInPage } from '../pages/SignInPage';

function DefaultAuthenticatedPage() {
    const auth = useAuth();
    const goesToDashboard = auth.user?.roles.some((role) => role === 'admin' || role === 'accountant');

    return <Navigate replace to={goesToDashboard ? '/dashboard' : '/invoices'} />;
}

export function AppRouter() {
    return (
        <Routes>
            <Route path="/sign-in" element={<SignInPage />} />

            <Route element={<ProtectedRoute />}>
                <Route element={<AppShell />}>
                    <Route index element={<DefaultAuthenticatedPage />} />
                    <Route
                        path="/dashboard"
                        element={
                            <RoleBoundary allowedRoles={['admin', 'accountant']} fallbackPath="/invoices">
                                <DashboardPage />
                            </RoleBoundary>
                        }
                    />
                    <Route path="/invoices" element={<InvoicesPage />} />
                    <Route path="/expenses" element={<ExpensesPage />} />
                    <Route path="*" element={<NotFoundPage />} />
                </Route>
            </Route>
        </Routes>
    );
}
