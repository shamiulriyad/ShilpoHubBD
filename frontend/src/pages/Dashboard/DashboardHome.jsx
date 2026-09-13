import { Link } from 'react-router-dom';
import { PageHeader, AsyncState } from '../../components/ui';
import { DashboardCard, StatCard } from '../../components/cards';
import { useConversations } from '../../hooks/useMessaging';
import { useAuth } from '../../hooks/useAuth';
import { roleSidebars } from '../../data/navigation';
import { roleLabel } from '../../utils/roles';

const listOf = (data) => data?.items || data || [];
const flattenNavigation = (groups = []) =>
  groups.flatMap((group) => group.items || []).filter((item, index, items) =>
    items.findIndex((candidate) => candidate.path === item.path) === index,
  );

export default function DashboardHome() {
  const { activeRole, roles } = useAuth();
  const conversationsQuery = useConversations();
  const conversations = listOf(conversationsQuery.data);
  const unreadMessages = conversations.reduce((sum, conversation) => sum + (conversation.unreadCount || 0), 0);
  const workspaceLinks = flattenNavigation(roleSidebars[activeRole]?.nav).slice(0, 8);

  return (
    <div>
      <PageHeader
        title="Dashboard"
        description={activeRole ? `Your ${roleLabel(activeRole)} workspace and account activity.` : 'Your ShilpoHub workspace.'}
      />

      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-3">
        <StatCard label="Assigned Roles" value={roles.length} />
        <StatCard label="Conversations" value={conversations.length} />
        <StatCard label="Unread Messages" value={unreadMessages} />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <DashboardCard title="Workspace shortcuts" description="Features available from your active role navigation.">
          {workspaceLinks.length > 0 ? (
            <div className="grid gap-2 sm:grid-cols-2">
              {workspaceLinks.map((item) => (
                <Link
                  key={item.path}
                  to={item.path}
                  className="rounded-lg border border-border bg-background px-3 py-3 text-sm font-medium text-heading transition hover:border-primary/30 hover:bg-primary-soft"
                >
                  <span aria-hidden="true" className="mr-2">{item.icon}</span>
                  {item.label}
                </Link>
              ))}
            </div>
          ) : (
            <p className="text-sm text-body/60">No role-specific shortcuts are available for this account.</p>
          )}
        </DashboardCard>

        <DashboardCard title="Messages" description="Unread activity from your real conversations.">
          <AsyncState
            isLoading={conversationsQuery.isLoading}
            isError={conversationsQuery.isError}
            error={conversationsQuery.error}
          >
            <div className="space-y-3">
              {conversations.slice(0, 5).map((conversation) => (
                <div key={conversation.id} className="flex items-center justify-between gap-3 rounded-lg border border-border p-3">
                  <p className="min-w-0 truncate text-sm font-medium text-heading">
                    {conversation.title || conversation.otherUserName || 'Conversation'}
                  </p>
                  {conversation.unreadCount > 0 && (
                    <span className="shrink-0 rounded-full bg-primary/10 px-2 py-1 text-xs font-semibold text-primary">
                      {conversation.unreadCount} unread
                    </span>
                  )}
                </div>
              ))}
              {conversations.length === 0 && <p className="text-sm text-body/60">No conversations yet.</p>}
            </div>
          </AsyncState>
        </DashboardCard>
      </div>
    </div>
  );
}
