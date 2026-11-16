import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type { SignatureRequest, SignatureSessionResult } from '../types/billing';

export interface CreateSignatureSessionRequest {
  contractId: string;
  signerName: string;
  signerEmail: string;
}

export const eSignatureService = {
  async sendSignatureRequest(data: CreateSignatureSessionRequest): Promise<ApiResponse<SignatureSessionResult>> {
    const res = await apiClient.post<ApiResponse<SignatureSessionResult>>('/e-signature/send', data);
    return res.data;
  },

  async getContractSignatures(contractId: string): Promise<ApiResponse<SignatureRequest[]>> {
    const res = await apiClient.get<ApiResponse<SignatureRequest[]>>(`/e-signature/contract/${contractId}`);
    return res.data;
  },

  async syncStatus(signatureRequestId: string): Promise<ApiResponse<SignatureRequest>> {
    const res = await apiClient.post<ApiResponse<SignatureRequest>>(`/e-signature/sync/${signatureRequestId}`);
    return res.data;
  },
};
