import { Link } from 'react-router-dom';

export function NotFoundPage() {
    return (
        <section className="card">
            <span className="card__eyebrow">Not found</span>
            <h3>That route is outside the current bootstrap.</h3>
            <p>
                Return to the dashboard and keep building from the scaffold that is already wired into the app
                shell.
            </p>
            <Link className="inline-link" to="/dashboard">
                Back to dashboard
            </Link>
        </section>
    );
}
