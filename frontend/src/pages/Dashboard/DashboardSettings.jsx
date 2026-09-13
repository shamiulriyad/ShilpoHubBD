import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageHeader, Button } from '../../components/ui';
import { DashboardCard } from '../../components/cards';
import { useTheme } from '../../hooks/useTheme';
import { useAuth } from '../../hooks/useAuth';
import { getApiErrorMessage } from '../../utils/apiError';
import { roleHomePath, roleLabel } from '../../utils/roles';

export default function DashboardSettings() {
  const navigate = useNavigate();
  const { theme, setTheme } = useTheme();
  const { roles, activeRole, switchRole } = useAuth();
  const [switchingRole, setSwitchingRole] = useState(null);
  const [switchError, setSwitchError] = useState('');

  const handleRoleSwitch = async (role) => {
    if (role === activeRole || switchingRole) return;
    setSwitchError('');
    setSwitchingRole(role);
    try {
      await switchRole(role);
      navigate(roleHomePath(role), { replace: true });
    } catch (error) {
      setSwitchError(getApiErrorMessage(error, 'Unable to switch workspace.'));
    } finally {
      setSwitchingRole(null);
    }
  };

  return (
    <div>
      <PageHeader title="Settings" description="Manage preferences that are actually supported by this application." />
      <div className="space-y-4">
        <DashboardCard title="Appearance" description="Choose the interface theme stored on this device.">
          <div className="flex flex-wrap gap-2" role="group" aria-label="Appearance theme">
            {['light', 'dark'].map((option) => (
              <Button
                key={option}
                type="button"
                variant={theme === option ? 'primary' : 'secondary'}
                size="sm"
                onClick={() => setTheme(option)}
                aria-pressed={theme === option}
              >
                {option === 'light' ? 'Light' : 'Dark'}
              </Button>
            ))}
          </div>
        </DashboardCard>

        <DashboardCard title="Active workspace" description="Switch between roles already granted to your account.">
          <div className="flex flex-wrap gap-2">
            {roles.map((role) => (
              <Button
                key={role}
                type="button"
                variant={role === activeRole ? 'primary' : 'secondary'}
                size="sm"
                disabled={Boolean(switchingRole) || role === activeRole}
                onClick={() => handleRoleSwitch(role)}
              >
                {switchingRole === role ? 'Switching…' : roleLabel(role)}
              </Button>
            ))}
          </div>
          {switchError && <p role="alert" className="mt-3 text-sm text-error">{switchError}</p>}
          {roles.length <= 1 && (
            <p className="mt-3 text-xs text-body/55">This account currently has one assigned role, so there is no alternate workspace to switch to.</p>
          )}
        </DashboardCard>
      </div>
    </div>
  );
}
