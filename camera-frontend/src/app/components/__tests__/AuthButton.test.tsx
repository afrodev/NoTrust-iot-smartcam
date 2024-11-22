import '@testing-library/jest-dom';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import AuthButton from '../AuthButton';
import { AuthProvider } from '../../context/AuthContext';

// Extend Jest matchers
declare global {
  namespace jest {
    interface Matchers<R> {
      toBeInTheDocument(): R;
      toHaveTextContent(text: string): R;
      toBeDisabled(): R;
    }
  }
}

// Mock fetch globally
global.fetch = jest.fn(() =>
  Promise.resolve({
    ok: true,
  } as Response)
);

describe('AuthButton', () => {
  beforeEach(() => {
    (fetch as jest.Mock).mockClear();
  });

  it('renders sign in button when not authenticated', () => {
    render(
      <AuthProvider>
        <AuthButton />
      </AuthProvider>
    );
    
    expect(screen.getByRole('button')).toHaveTextContent('Sign In');
  });

  it('shows loading state while authenticating', async () => {
    render(
      <AuthProvider>
        <AuthButton />
      </AuthProvider>
    );
    
    const button = screen.getByRole('button');
    fireEvent.click(button);
    
    expect(button).toHaveTextContent('Loading...');
    expect(button).toBeDisabled();
    
    await waitFor(() => {
      expect(button).not.toHaveTextContent('Loading...');
    });
  });

  it('calls sign in API when clicked', async () => {
    render(
      <AuthProvider>
        <AuthButton />
      </AuthProvider>
    );
    
    const button = screen.getByRole('button');
    fireEvent.click(button);
    
    await waitFor(() => {
      expect(fetch).toHaveBeenCalledWith('/api/auth/signin', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
    });
  });

  it('changes to sign out button after successful sign in', async () => {
    render(
      <AuthProvider>
        <AuthButton />
      </AuthProvider>
    );
    
    const button = screen.getByRole('button');
    fireEvent.click(button);
    
    await waitFor(() => {
      expect(button).toHaveTextContent('Sign Out');
    });
  });
});
