import { useMutation } from '@tanstack/react-query';
import { arCraftScanService } from '../services/arCraftScanService';

export function useArCraftScan() {
  return useMutation({ mutationFn: (code) => arCraftScanService.scan(code) });
}
