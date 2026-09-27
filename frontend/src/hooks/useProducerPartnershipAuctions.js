import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { producerPartnershipAuctionService } from '../services/producerPartnershipAuctionService';

export function useProducerPartnershipAuctions(params = {}) {
  return useQuery({ queryKey: ['producer-partnership-auctions', params], queryFn: () => producerPartnershipAuctionService.list(params) });
}

export function useProducerPartnershipAuction(id) {
  return useQuery({
    queryKey: ['producer-partnership-auctions', id],
    queryFn: () => producerPartnershipAuctionService.getById(id),
    enabled: Boolean(id),
  });
}

export function useProducerPartnershipAuctionLots(auctionId) {
  return useQuery({
    queryKey: ['producer-partnership-auctions', auctionId, 'lots'],
    queryFn: () => producerPartnershipAuctionService.getLots(auctionId),
    enabled: Boolean(auctionId),
    refetchInterval: 15000,
  });
}

export function useProducerPartnershipAuctionLot(auctionId, lotId) {
  return useQuery({
    queryKey: ['producer-partnership-auctions', auctionId, 'lots', lotId],
    queryFn: () => producerPartnershipAuctionService.getLot(auctionId, lotId),
    enabled: Boolean(auctionId) && Boolean(lotId),
    refetchInterval: 10000,
  });
}

export function useProducerPartnershipAuctionParticipants(auctionId) {
  return useQuery({
    queryKey: ['producer-partnership-auctions', auctionId, 'participants'],
    queryFn: () => producerPartnershipAuctionService.getParticipants(auctionId),
    enabled: Boolean(auctionId),
  });
}

export function useMyParticipantStatus(auctionId) {
  return useQuery({
    queryKey: ['producer-partnership-auctions', auctionId, 'participants', 'me'],
    queryFn: () => producerPartnershipAuctionService.getMineAsParticipant(auctionId),
    enabled: Boolean(auctionId),
  });
}

export function useMyAuctionBids(auctionId, lotId) {
  return useQuery({
    queryKey: ['producer-partnership-auctions', auctionId, 'my-bids', lotId],
    queryFn: () => producerPartnershipAuctionService.getMyBids(auctionId, lotId),
    enabled: Boolean(auctionId),
    refetchInterval: 15000,
  });
}

export function useBidHistory(auctionId, lotId) {
  return useQuery({
    queryKey: ['producer-partnership-auctions', auctionId, 'lots', lotId, 'bid-history'],
    queryFn: () => producerPartnershipAuctionService.getBidHistory(auctionId, lotId),
    enabled: Boolean(auctionId) && Boolean(lotId),
  });
}

export function useProducerPartnershipAuctionMutations(auctionId) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['producer-partnership-auctions'] });

  const create = useMutation({ mutationFn: (payload) => producerPartnershipAuctionService.create(payload), onSuccess: invalidate });
  const update = useMutation({ mutationFn: ({ id, payload }) => producerPartnershipAuctionService.update(id, payload), onSuccess: invalidate });
  const schedule = useMutation({ mutationFn: (id) => producerPartnershipAuctionService.schedule(id), onSuccess: invalidate });
  const openRegistration = useMutation({ mutationFn: (id) => producerPartnershipAuctionService.openRegistration(id), onSuccess: invalidate });
  const goLive = useMutation({ mutationFn: (id) => producerPartnershipAuctionService.goLive(id), onSuccess: invalidate });
  const end = useMutation({ mutationFn: (id) => producerPartnershipAuctionService.end(id), onSuccess: invalidate });
  const cancel = useMutation({ mutationFn: (id) => producerPartnershipAuctionService.cancel(id), onSuccess: invalidate });

  const addLot = useMutation({ mutationFn: (producerId) => producerPartnershipAuctionService.addLot(auctionId, producerId), onSuccess: invalidate });
  const removeLot = useMutation({ mutationFn: (lotId) => producerPartnershipAuctionService.removeLot(auctionId, lotId), onSuccess: invalidate });

  const decideParticipant = useMutation({
    mutationFn: ({ participantId, approve, notes }) => producerPartnershipAuctionService.decideParticipant(auctionId, participantId, approve, notes),
    onSuccess: invalidate,
  });

  return { create, update, schedule, openRegistration, goLive, end, cancel, addLot, removeLot, decideParticipant };
}

export function useProducerPartnershipParticipation(auctionId) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['producer-partnership-auctions', auctionId] });

  const apply = useMutation({ mutationFn: () => producerPartnershipAuctionService.apply(auctionId), onSuccess: invalidate });
  const placeBid = useMutation({
    mutationFn: ({ lotId, amount }) => producerPartnershipAuctionService.placeBid(auctionId, lotId, amount),
    onSuccess: invalidate,
  });

  return { apply, placeBid };
}
