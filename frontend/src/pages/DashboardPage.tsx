import { useEffect, useState } from 'react';
import { apiClient } from '../lib/http';

type ApiHealthState = 'checking' | 'online' | 'offline';

const summaryCards = [
    {
        label: 'Backend host',
        value: 'Controller-based .NET 10 API',
        detail: 'OpenAPI, ProblemDetails, health checks, and CORS are enabled for local work.',
    },
    {
        label: 'Frontend shell',
        value: 'React + Vite + Router',
        detail: 'Feature routes are in place for dashboard, invoices, expenses, and team access.',
    },
    {
        label: 'Local stack',
        value: 'PostgreSQL + user secrets',
        detail: 'Development credentials stay outside the repo while compose handles the database.',
    },
];

export function DashboardPage() {
    const [apiHealth, setApiHealth] = useState<ApiHealthState>('checking');

    useEffect(() => {
        let cancelled = false;

        apiClient
            .get('/health')
            .then(() => {
                if (!cancelled) {
                    setApiHealth('online');
                }
            })
            .catch(() => {
                if (!cancelled) {
                    setApiHealth('offline');
                }
            });

        return () => {
            cancelled = true;
        };
    }, []);

    return (
        <section className="page-grid">
            <section className="card card--highlight">
                <span className="card__eyebrow">System probe</span>
                <h3>API health status: {apiHealth}</h3>
                <p>
                    The frontend pings <code>/health</code> through the configured API base URL so the shell
                    exposes integration drift immediately during local setup.
                </p>
            </section>

            <section className="stats-grid">
                {summaryCards.map((card) => (
                    <article key={card.label} className="card">
                        <span className="card__eyebrow">{card.label}</span>
                        <h3>{card.value}</h3>
                        <p>{card.detail}</p>
                    </article>
                ))}
            </section>

            <section className="card">
                <span className="card__eyebrow">Next implementation slices</span>
                <ul className="feature-list">
                    <li>Identity, JWT auth, and seeded company administration</li>
                    <li>Invoice and expense aggregates with validation and totals</li>
                    <li>Malaysia-specific export fields and dashboard metrics</li>
                </ul>
            </section>
        </section>
    );
}
