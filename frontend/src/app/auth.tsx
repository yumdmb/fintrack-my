import {
    createContext,
    useContext,
    useEffect,
    useState,
    type PropsWithChildren
} from 'react';
import { getCurrentUser, signIn, type CurrentUserResponse } from '../lib/api';
import { setApiAccessToken } from '../lib/http';

type AuthStatus = 'booting' | 'anonymous' | 'authenticated';

interface AuthContextValue {
    status: AuthStatus;
    user: CurrentUserResponse | null;
    accessToken: string | null;
    signInWithPassword: (email: string, password: string) => Promise<void>;
    signOut: () => void;
}

const authStorageKey = 'fintrack.access-token';

const AuthContext = createContext<AuthContextValue | null>(null);

function readStoredToken() {
    return window.localStorage.getItem(authStorageKey);
}

function persistToken(token: string | null) {
    if (token) {
        window.localStorage.setItem(authStorageKey, token);
        return;
    }

    window.localStorage.removeItem(authStorageKey);
}

export function AuthProvider({ children }: PropsWithChildren) {
    const [status, setStatus] = useState<AuthStatus>('booting');
    const [user, setUser] = useState<CurrentUserResponse | null>(null);
    const [accessToken, setAccessToken] = useState<string | null>(null);

    useEffect(() => {
        const token = readStoredToken();
        if (!token) {
            setApiAccessToken(null);
            setStatus('anonymous');
            return;
        }

        setApiAccessToken(token);
        setAccessToken(token);

        getCurrentUser()
            .then((currentUser) => {
                setUser(currentUser);
                setStatus('authenticated');
            })
            .catch(() => {
                persistToken(null);
                setApiAccessToken(null);
                setAccessToken(null);
                setUser(null);
                setStatus('anonymous');
            });
    }, []);

    async function signInWithPassword(email: string, password: string) {
        const result = await signIn({ email, password });

        persistToken(result.accessToken);
        setApiAccessToken(result.accessToken);
        setAccessToken(result.accessToken);
        setUser(result.user);
        setStatus('authenticated');
    }

    function signOut() {
        persistToken(null);
        setApiAccessToken(null);
        setAccessToken(null);
        setUser(null);
        setStatus('anonymous');
    }

    return (
        <AuthContext.Provider
            value={{
                status,
                user,
                accessToken,
                signInWithPassword,
                signOut
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within AuthProvider.');
    }

    return context;
}
