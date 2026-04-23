import { Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from '../components/AppShell';
import { DashboardPage } from '../pages/DashboardPage';
import { ExpensesPage } from '../pages/ExpensesPage';
import { InvoicesPage } from '../pages/InvoicesPage';
import { NotFoundPage } from '../pages/NotFoundPage';
import { TeamPage } from '../pages/TeamPage';

export function AppRouter() {
    return (
        <Routes>
            <Route element={<AppShell />}>
                <Route index element={<Navigate replace to="/dashboard" />} />
                <Route path="/dashboard" element={<DashboardPage />} />
                <Route path="/invoices" element={<InvoicesPage />} />
                <Route path="/expenses" element={<ExpensesPage />} />
                <Route path="/team" element={<TeamPage />} />
                <Route path="*" element={<NotFoundPage />} />
            </Route>
        </Routes>
    );
}
