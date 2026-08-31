import { useQuery } from '@tanstack/react-query';
import { workshopGalleryService } from '../services/workshopGalleryService';

export function useWorkshopGallery(producerId) {
  return useQuery({
    queryKey: ['workshop-gallery', producerId],
    queryFn: () => workshopGalleryService.listForProducer(producerId),
    enabled: Boolean(producerId),
  });
}
