import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { UserDto, LoginRequest, RegisterRequest } from '../types';
import { login as loginApi, register as registerApi, getCurrentUser } from '../api/auth';

interface AuthContextType {
  user: UserDto | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (request: LoginRequest) => Promise<{ success: boolean; message?: string }>;
  register: (request: RegisterRequest) => Promise<{ success: boolean; message?: string }>;
  logout: () => void;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const AUTH_STORAGE_KEY = 'skillmatrix_auth';

interface StoredAuth {
  user: UserDto;
  token: string;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Load user from localStorage on mount
  useEffect(() => {
    const loadUser = async () => {
      try {
        const stored = localStorage.getItem(AUTH_STORAGE_KEY);
        if (stored) {
          const { user: storedUser } = JSON.parse(stored) as StoredAuth;
          // Verify user still exists
          const freshUser = await getCurrentUser(storedUser.id);
          setUser(freshUser);
        }
      } catch {
        // Token invalid or user not found
        localStorage.removeItem(AUTH_STORAGE_KEY);
      } finally {
        setIsLoading(false);
      }
    };

    loadUser();
  }, []);

  const login = async (request: LoginRequest): Promise<{ success: boolean; message?: string }> => {
    try {
      const response = await loginApi(request);
      if (response.success && response.user && response.token) {
        setUser(response.user);
        localStorage.setItem(
          AUTH_STORAGE_KEY,
          JSON.stringify({ user: response.user, token: response.token })
        );
        return { success: true };
      }
      return { success: false, message: response.message };
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } };
      return {
        success: false,
        message: err?.response?.data?.message || 'Login failed',
      };
    }
  };

  const register = async (
    request: RegisterRequest
  ): Promise<{ success: boolean; message?: string }> => {
    try {
      const response = await registerApi(request);
      if (response.success && response.user && response.token) {
        setUser(response.user);
        localStorage.setItem(
          AUTH_STORAGE_KEY,
          JSON.stringify({ user: response.user, token: response.token })
        );
        return { success: true };
      }
      return { success: false, message: response.message };
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } };
      return {
        success: false,
        message: err?.response?.data?.message || 'Registration failed',
      };
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem(AUTH_STORAGE_KEY);
  };

  const refreshUser = async () => {
    if (user?.id) {
      try {
        const freshUser = await getCurrentUser(user.id);
        setUser(freshUser);
        // Update localStorage
        const stored = localStorage.getItem(AUTH_STORAGE_KEY);
        if (stored) {
          const { token } = JSON.parse(stored) as StoredAuth;
          localStorage.setItem(
            AUTH_STORAGE_KEY,
            JSON.stringify({ user: freshUser, token })
          );
        }
      } catch {
        // Failed to refresh user
      }
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        login,
        register,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
