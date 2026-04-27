import { Link } from 'react-router-dom';

export function NotFoundPage() {
    return (
        <section className="card" style={{ maxWidth: '480px' }}>
            <span className="eyebrow">404</span>
            <h3>Page not found</h3>
            <p style={{ marginTop: '0.5rem' }}>
                This route does not exist. Return to the workspace to continue.
            </p>
            <Link className="inline-link" to="/" style={{ display: 'inline-block', marginTop: '1rem' }}>
                Back to workspace
            </Link>
        </section>
    );
}
