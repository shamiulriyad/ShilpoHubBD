import { useMyLogisticsPartnerProfile } from './useLogisticsPartners';
import { useWarehouses } from './useWarehouses';
import { useShipments } from './useShipments';
import { usePickupRequests } from './usePickupRequests';
import { useLogisticsReturns } from './useLogisticsReturns';
import { useDeliveryRoutes } from './useDeliveryRoutes';

export function useLogisticsDashboardStats() {
  const profile = useMyLogisticsPartnerProfile();
  const warehouses = useWarehouses({ pageSize: 1 });
  const activeShipments = useShipments({ pageSize: 1, status: 'InTransit' });
  const pendingPickups = usePickupRequests({ pageSize: 1, status: 'Requested' });
  const openReturns = useLogisticsReturns({ pageSize: 1, status: 'Requested' });
  const activeRoutes = useDeliveryRoutes({ pageSize: 1, status: 'InProgress' });

  return {
    profile,
    stats: {
      warehouseCount: warehouses.data?.totalCount,
      activeShipmentCount: activeShipments.data?.totalCount,
      pendingPickupCount: pendingPickups.data?.totalCount,
      openReturnCount: openReturns.data?.totalCount,
      activeRouteCount: activeRoutes.data?.totalCount,
    },
    isLoading: warehouses.isLoading || activeShipments.isLoading || pendingPickups.isLoading || openReturns.isLoading || activeRoutes.isLoading,
  };
}
