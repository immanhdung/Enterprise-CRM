import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type {
  ServiceCategory,
  ServiceItem,
  ServicePriceHistory,
  PriceList,
} from '../types/billing';

export interface CreateServiceRequest {
  code: string;
  name: string;
  description?: string;
  unit: string;
  basePrice: number;
  costPrice: number;
  categoryId?: string;
}

export interface UpdateServiceRequest {
  name: string;
  description?: string;
  unit: string;
  basePrice: number;
  costPrice: number;
  categoryId?: string;
  isActive: boolean;
  priceChangeReason?: string;
}

export interface CreatePriceListRequest {
  code: string;
  name: string;
  description?: string;
  currency: string;
  startDate?: string;
  endDate?: string;
  isDefault: boolean;
  isActive: boolean;
  items: Array<{
    serviceId: string;
    unitPrice: number;
    minQuantity: number;
    maxDiscountRate: number;
  }>;
}

export const serviceCatalogService = {
  // Categories
  async getCategories(includeInactive = false): Promise<ApiResponse<ServiceCategory[]>> {
    const res = await apiClient.get<ApiResponse<ServiceCategory[]>>('/service-categories', {
      params: { includeInactive },
    });
    return res.data;
  },

  async createCategory(data: { code: string; name: string; description?: string; orderIndex?: number }): Promise<ApiResponse<ServiceCategory>> {
    const res = await apiClient.post<ApiResponse<ServiceCategory>>('/service-categories', data);
    return res.data;
  },

  async updateCategory(id: string, data: { name: string; description?: string; orderIndex?: number; isActive: boolean }): Promise<ApiResponse<ServiceCategory>> {
    const res = await apiClient.put<ApiResponse<ServiceCategory>>(`/service-categories/${id}`, data);
    return res.data;
  },

  async deleteCategory(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/service-categories/${id}`);
    return res.data;
  },

  // Services
  async getServices(params?: {
    search?: string;
    categoryId?: string;
    isActive?: boolean;
  }): Promise<ApiResponse<ServiceItem[]>> {
    const res = await apiClient.get<ApiResponse<ServiceItem[]>>('/services', { params });
    return res.data;
  },

  async getServiceById(id: string): Promise<ApiResponse<ServiceItem>> {
    const res = await apiClient.get<ApiResponse<ServiceItem>>(`/services/${id}`);
    return res.data;
  },

  async createService(data: CreateServiceRequest): Promise<ApiResponse<ServiceItem>> {
    const res = await apiClient.post<ApiResponse<ServiceItem>>('/services', data);
    return res.data;
  },

  async updateService(id: string, data: UpdateServiceRequest): Promise<ApiResponse<ServiceItem>> {
    const res = await apiClient.put<ApiResponse<ServiceItem>>(`/services/${id}`, data);
    return res.data;
  },

  async deleteService(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/services/${id}`);
    return res.data;
  },

  async getPriceHistory(serviceId: string): Promise<ApiResponse<ServicePriceHistory[]>> {
    const res = await apiClient.get<ApiResponse<ServicePriceHistory[]>>(`/services/${serviceId}/price-history`);
    return res.data;
  },

  async getEffectivePrice(serviceId: string, priceListId?: string, quantity = 1): Promise<ApiResponse<number | null>> {
    const res = await apiClient.get<ApiResponse<number | null>>(`/services/${serviceId}/effective-price`, {
      params: { priceListId, quantity },
    });
    return res.data;
  },

  // Price Lists
  async getPriceLists(includeInactive = false): Promise<ApiResponse<PriceList[]>> {
    const res = await apiClient.get<ApiResponse<PriceList[]>>('/price-lists', {
      params: { includeInactive },
    });
    return res.data;
  },

  async getPriceListById(id: string): Promise<ApiResponse<PriceList>> {
    const res = await apiClient.get<ApiResponse<PriceList>>(`/price-lists/${id}`);
    return res.data;
  },

  async createPriceList(data: CreatePriceListRequest): Promise<ApiResponse<PriceList>> {
    const res = await apiClient.post<ApiResponse<PriceList>>('/price-lists', data);
    return res.data;
  },

  async updatePriceList(id: string, data: CreatePriceListRequest): Promise<ApiResponse<PriceList>> {
    const res = await apiClient.put<ApiResponse<PriceList>>(`/price-lists/${id}`, data);
    return res.data;
  },

  async deletePriceList(id: string): Promise<ApiResponse<boolean>> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/price-lists/${id}`);
    return res.data;
  },
};
