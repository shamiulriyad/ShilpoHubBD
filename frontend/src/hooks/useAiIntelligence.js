import { useMutation } from '@tanstack/react-query';
import { aiIntelligenceService } from '../services/aiIntelligenceService';

export function useAiIntelligenceTools() {
  const rankSuppliers = useMutation({ mutationFn: (payload) => aiIntelligenceService.rankSuppliers(payload) });
  const predictQuality = useMutation({ mutationFn: (producerId) => aiIntelligenceService.predictQuality(producerId) });
  const forecastPrice = useMutation({ mutationFn: (payload) => aiIntelligenceService.forecastPrice(payload) });
  const predictDelivery = useMutation({ mutationFn: (payload) => aiIntelligenceService.predictDelivery(payload) });
  const assessRisk = useMutation({ mutationFn: (producerId) => aiIntelligenceService.assessRisk(producerId) });

  return { rankSuppliers, predictQuality, forecastPrice, predictDelivery, assessRisk };
}
