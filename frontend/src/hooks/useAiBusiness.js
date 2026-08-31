import { useMutation } from '@tanstack/react-query';
import { aiBusinessService } from '../services/aiBusinessService';

export function useAiBusinessTools() {
  const suggestPrice = useMutation({ mutationFn: (payload) => aiBusinessService.suggestPrice(payload) });
  const generateDescription = useMutation({ mutationFn: (payload) => aiBusinessService.generateDescription(payload) });
  const translate = useMutation({ mutationFn: (payload) => aiBusinessService.translate(payload) });
  const forecastDemand = useMutation({ mutationFn: (payload) => aiBusinessService.forecastDemand(payload) });
  const planProduction = useMutation({ mutationFn: (payload) => aiBusinessService.planProduction(payload) });
  const forecastMaterials = useMutation({ mutationFn: (payload) => aiBusinessService.forecastMaterials(payload) });
  const predictSeasonalTrend = useMutation({ mutationFn: (payload) => aiBusinessService.predictSeasonalTrend(payload) });
  const generateSalesInsights = useMutation({ mutationFn: (payload) => aiBusinessService.generateSalesInsights(payload) });

  return {
    suggestPrice,
    generateDescription,
    translate,
    forecastDemand,
    planProduction,
    forecastMaterials,
    predictSeasonalTrend,
    generateSalesInsights,
  };
}
