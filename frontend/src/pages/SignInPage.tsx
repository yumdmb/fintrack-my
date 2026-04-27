import { useState, type FormEvent } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../app/auth';
import { ApiRequestError } from '../lib/api';

export function SignInPage() {
    const auth = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const [email, setEmail] = useState('admin@fintrack.local');
    const [password, setPassword] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    if (auth.status === 'authenticated') {
        return <Navigate replace to="/dashboard" />;
    }

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setIsSubmitting(true);
        setErrorMessage(null);

        try {
            await auth.signInWithPassword(email, password);
            const next = typeof location.state?.from === 'string' ? location.state.from : '/dashboard';
            navigate(next, { replace: true });
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setErrorMessage(error.detail ?? error.message);
            }
            else {
                setErrorMessage('Unable to sign in right now.');
            }
        }
        finally {
            setIsSubmitting(false);
        }
    }

    return (
        <main className="auth-layout">
            <section className="auth-hero">
                <h1>Sign in to your finance workspace.</h1>
                <p>
                    Manage invoices, track expenses, finalize exports, and review company metrics
                    from the ASP.NET backend.
                </p>

                <div className="auth-features">
                    <div className="auth-feature">
                        <span className="auth-feature__label">Auth</span>
                        <span className="auth-feature__value">JWT with current-user bootstrap</span>
                    </div>
                    <div className="auth-feature">
                        <span className="auth-feature__label">Workflows</span>
                        <span className="auth-feature__value">Invoices, expenses, exports, reporting</span>
                    </div>
                    <div className="auth-feature">
                        <span className="auth-feature__label">Getting started</span>
                        <span className="auth-feature__value">Use admin credentials from README.md</span>
                    </div>
                </div>
            </section>

            <section className="auth-card">
                <div>
                    <span className="eyebrow">Sign in</span>
                    <h2>Enter your credentials</h2>
                </div>

                <form className="form-stack" onSubmit={handleSubmit} style={{ marginTop: '1.5rem' }}>
                    <label className="field">
                        <span className="field__label">Email</span>
                        <input
                            autoComplete="email"
                            className="field__input"
                            type="email"
                            value={email}
                            onChange={(event) => setEmail(event.target.value)}
                        />
                    </label>

                    <label className="field">
                        <span className="field__label">Password</span>
                        <input
                            autoComplete="current-password"
                            className="field__input"
                            type="password"
                            value={password}
                            onChange={(event) => setPassword(event.target.value)}
                        />
                    </label>

                    {errorMessage ? <p className="status status--error">{errorMessage}</p> : null}

                    <button className="button button--primary button--block" disabled={isSubmitting} type="submit">
                        {isSubmitting ? 'Signing in...' : 'Sign in'}
                    </button>
                </form>
            </section>
        </main>
    );
}
