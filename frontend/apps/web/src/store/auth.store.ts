import { create } from 'zustand';
import { CurrentUserResponse } from '@platform/contracts';

interface AuthState {
  user: CurrentUserResponse | null;
  isAuthenticated: boolean;
  setUser: (user: CurrentUserResponse | null) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  setUser: (user) => set({ user, isAuthenticated: !!user }),
  logout: () => set({ user: null, isAuthenticated: false }),
}));
