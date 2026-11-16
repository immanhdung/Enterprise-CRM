import React, { useState, useEffect, useCallback } from 'react';
import {
  Plus,
  Search,
  Layers,
  History,
  Edit,
  Trash2,
  CheckCircle,
  XCircle,
  RefreshCw,
  FolderPlus,
} from 'lucide-react';
import { serviceCatalogService, type CreateServiceRequest, type UpdateServiceRequest } from '../../services/serviceCatalogService';
import type { ServiceItem, ServiceCategory, ServicePriceHistory } from '../../types/billing';
import { Modal } from '../../components/common/Modal';

export const ServicesPage: React.FC = () => {
  const [services, setServices] = useState<ServiceItem[]>([]);
  const [categories, setCategories] = useState<ServiceCategory[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');

  // Modals state
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingService, setEditingService] = useState<ServiceItem | null>(null);
  const [priceHistoryService, setPriceHistoryService] = useState<ServiceItem | null>(null);
  const [priceHistories, setPriceHistories] = useState<ServicePriceHistory[]>([]);
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');
  const [newCategoryCode, setNewCategoryCode] = useState('');

  // Form states
  const [formData, setFormData] = useState<CreateServiceRequest>({
    code: '',
    name: '',
    description: '',
    unit: 'Project',
    basePrice: 0,
    costPrice: 0,
    categoryId: '',
  });

  const [editFormData, setEditFormData] = useState<UpdateServiceRequest>({
    name: '',
    description: '',
    unit: 'Project',
    basePrice: 0,
    costPrice: 0,
    categoryId: '',
    isActive: true,
    priceChangeReason: '',
  });

  const loadData = useCallback(async () => {
    setIsLoading(true);
    try {
      const [svcRes, catRes] = await Promise.all([
        serviceCatalogService.getServices({
          search: search || undefined,
          categoryId: selectedCategory || undefined,
          isActive: selectedStatus ? selectedStatus === 'active' : undefined,
        }),
        serviceCatalogService.getCategories(true),
      ]);

      if (svcRes.data) setServices(svcRes.data);
      if (catRes.data) setCategories(catRes.data);
    } catch (err) {
      console.error('Failed to load services data', err);
    } finally {
      setIsLoading(false);
    }
  }, [search, selectedCategory, selectedStatus]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleCreateService = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await serviceCatalogService.createService({
        ...formData,
        categoryId: formData.categoryId || undefined,
      });
      setIsCreateOpen(false);
      setFormData({
        code: '',
        name: '',
        description: '',
        unit: 'Project',
        basePrice: 0,
        costPrice: 0,
        categoryId: '',
      });
      loadData();
    } catch (err) {
      console.error('Failed to create service', err);
    }
  };

  const handleOpenEdit = (svc: ServiceItem) => {
    setEditingService(svc);
    setEditFormData({
      name: svc.name,
      description: svc.description || '',
      unit: svc.unit,
      basePrice: svc.basePrice,
      costPrice: svc.costPrice,
      categoryId: svc.categoryId || '',
      isActive: svc.isActive,
      priceChangeReason: '',
    });
  };

  const handleUpdateService = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingService) return;
    try {
      await serviceCatalogService.updateService(editingService.id, {
        ...editFormData,
        categoryId: editFormData.categoryId || undefined,
      });
      setEditingService(null);
      loadData();
    } catch (err) {
      console.error('Failed to update service', err);
    }
  };

  const handleDeleteService = async (id: string) => {
    if (!confirm('Are you sure you want to deactivate this service?')) return;
    try {
      await serviceCatalogService.deleteService(id);
      loadData();
    } catch (err) {
      console.error('Failed to delete service', err);
    }
  };

  const handleViewPriceHistory = async (svc: ServiceItem) => {
    setPriceHistoryService(svc);
    try {
      const res = await serviceCatalogService.getPriceHistory(svc.id);
      if (res.data) setPriceHistories(res.data);
    } catch (err) {
      console.error('Failed to load price history', err);
    }
  };

  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await serviceCatalogService.createCategory({
        code: newCategoryCode.toUpperCase(),
        name: newCategoryName,
      });
      setNewCategoryName('');
      setNewCategoryCode('');
      setIsCategoryModalOpen(false);
      loadData();
    } catch (err) {
      console.error('Failed to create category', err);
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 flex items-center gap-2">
            <Layers className="w-7 h-7 text-blue-600" />
            Service Catalog & Rate Cards
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Configure deliverable service items, standardized units, base tariffs, and track price changes.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => setIsCategoryModalOpen(true)}
            className="flex items-center gap-2 px-3 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-medium text-sm rounded-lg transition-colors"
          >
            <FolderPlus className="w-4 h-4" />
            Manage Categories
          </button>
          <button
            onClick={() => setIsCreateOpen(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium text-sm rounded-lg shadow-sm transition-colors"
          >
            <Plus className="w-4 h-4" />
            New Service Item
          </button>
        </div>
      </div>

      {/* Filters Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-wrap gap-4 items-center justify-between">
        <div className="flex flex-wrap items-center gap-3 flex-1 min-w-[280px]">
          <div className="relative flex-1 max-w-md">
            <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
            <input
              type="text"
              placeholder="Search by code, service name, or description..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-9 pr-4 py-2 border border-slate-300 rounded-lg text-sm focus:outline-hidden focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <select
            value={selectedCategory}
            onChange={(e) => setSelectedCategory(e.target.value)}
            className="px-3 py-2 border border-slate-300 rounded-lg text-sm bg-white text-slate-700 focus:outline-hidden focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Categories</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>

          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
            className="px-3 py-2 border border-slate-300 rounded-lg text-sm bg-white text-slate-700 focus:outline-hidden focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Statuses</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
        </div>

        <button
          onClick={loadData}
          className="p-2 border border-slate-300 rounded-lg hover:bg-slate-50 text-slate-600 transition-colors"
          title="Refresh services"
        >
          <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
        </button>
      </div>

      {/* Data Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-slate-700">
            <thead className="bg-slate-50 border-b border-slate-200 text-xs uppercase font-semibold text-slate-500 tracking-wider">
              <tr>
                <th className="px-6 py-3.5">Code</th>
                <th className="px-6 py-3.5">Service Details</th>
                <th className="px-6 py-3.5">Category</th>
                <th className="px-6 py-3.5">Unit</th>
                <th className="px-6 py-3.5 text-right">Base Price</th>
                <th className="px-6 py-3.5 text-right">Cost Price</th>
                <th className="px-6 py-3.5 text-center">Status</th>
                <th className="px-6 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {isLoading && services.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-6 py-10 text-center text-slate-400">
                    Loading services catalog...
                  </td>
                </tr>
              ) : services.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-6 py-12 text-center text-slate-400">
                    No service items found matching your filter criteria.
                  </td>
                </tr>
              ) : (
                services.map((svc) => (
                  <tr key={svc.id} className="hover:bg-slate-50/75 transition-colors">
                    <td className="px-6 py-4 font-mono font-medium text-blue-600">
                      {svc.code}
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-medium text-slate-900">{svc.name}</div>
                      {svc.description && (
                        <div className="text-xs text-slate-500 truncate max-w-xs">{svc.description}</div>
                      )}
                    </td>
                    <td className="px-6 py-4">
                      <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-700">
                        {svc.categoryName || svc.category || 'General'}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-slate-600">{svc.unit}</td>
                    <td className="px-6 py-4 text-right font-semibold text-slate-900">
                      {formatCurrency(svc.basePrice)}
                    </td>
                    <td className="px-6 py-4 text-right text-slate-500">
                      {formatCurrency(svc.costPrice)}
                    </td>
                    <td className="px-6 py-4 text-center">
                      {svc.isActive ? (
                        <span className="inline-flex items-center gap-1 text-emerald-600 text-xs font-medium">
                          <CheckCircle className="w-3.5 h-3.5" /> Active
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 text-rose-500 text-xs font-medium">
                          <XCircle className="w-3.5 h-3.5" /> Inactive
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 text-right space-x-2">
                      <button
                        onClick={() => handleViewPriceHistory(svc)}
                        title="View Price History"
                        className="p-1 text-slate-400 hover:text-blue-600 transition-colors"
                      >
                        <History className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleOpenEdit(svc)}
                        title="Edit Service"
                        className="p-1 text-slate-400 hover:text-amber-600 transition-colors"
                      >
                        <Edit className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleDeleteService(svc.id)}
                        title="Deactivate Service"
                        className="p-1 text-slate-400 hover:text-rose-600 transition-colors"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create Service Modal */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        title="Create Deliverable Service"
      >
        <form onSubmit={handleCreateService} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Service Code *</label>
              <input
                type="text"
                required
                placeholder="e.g. SVC-CLOUD-01"
                value={formData.code}
                onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm font-mono focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Unit of Measure *</label>
              <select
                value={formData.unit}
                onChange={(e) => setFormData({ ...formData, unit: e.target.value })}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              >
                <option value="Project">Project</option>
                <option value="Month">Month</option>
                <option value="Hour">Hour</option>
                <option value="Day">Day</option>
                <option value="License">License</option>
                <option value="Seat">Seat</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Service Name *</label>
            <input
              type="text"
              required
              placeholder="e.g. Enterprise Cloud Migration"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Category</label>
            <select
              value={formData.categoryId}
              onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Select Category --</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Base Price (VND) *</label>
              <input
                type="number"
                required
                min="0"
                step="100000"
                value={formData.basePrice}
                onChange={(e) => setFormData({ ...formData, basePrice: Number(e.target.value) })}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Internal Cost Price (VND)</label>
              <input
                type="number"
                min="0"
                step="100000"
                value={formData.costPrice}
                onChange={(e) => setFormData({ ...formData, costPrice: Number(e.target.value) })}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Description</label>
            <textarea
              rows={3}
              placeholder="Scope of work, deliverable details, or technical prerequisites..."
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="flex justify-end gap-3 pt-3 border-t border-slate-200">
            <button
              type="button"
              onClick={() => setIsCreateOpen(false)}
              className="px-4 py-2 border border-slate-300 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium shadow-sm"
            >
              Save Service
            </button>
          </div>
        </form>
      </Modal>

      {/* Edit Service Modal */}
      {editingService && (
        <Modal
          isOpen={true}
          onClose={() => setEditingService(null)}
          title={`Edit Service - ${editingService.code}`}
        >
          <form onSubmit={handleUpdateService} className="space-y-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Service Name *</label>
              <input
                type="text"
                required
                value={editFormData.name}
                onChange={(e) => setEditFormData({ ...editFormData, name: e.target.value })}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-slate-700 mb-1">Category</label>
                <select
                  value={editFormData.categoryId}
                  onChange={(e) => setEditFormData({ ...editFormData, categoryId: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">-- None --</option>
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-medium text-slate-700 mb-1">Unit</label>
                <select
                  value={editFormData.unit}
                  onChange={(e) => setEditFormData({ ...editFormData, unit: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                >
                  <option value="Project">Project</option>
                  <option value="Month">Month</option>
                  <option value="Hour">Hour</option>
                  <option value="Day">Day</option>
                  <option value="License">License</option>
                  <option value="Seat">Seat</option>
                </select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-slate-700 mb-1">Base Price (VND) *</label>
                <input
                  type="number"
                  required
                  min="0"
                  step="100000"
                  value={editFormData.basePrice}
                  onChange={(e) => setEditFormData({ ...editFormData, basePrice: Number(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-slate-700 mb-1">Cost Price (VND)</label>
                <input
                  type="number"
                  min="0"
                  step="100000"
                  value={editFormData.costPrice}
                  onChange={(e) => setEditFormData({ ...editFormData, costPrice: Number(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            {/* Price change reason for audit */}
            {editFormData.basePrice !== editingService.basePrice && (
              <div className="bg-amber-50 border border-amber-200 p-3 rounded-lg">
                <label className="block text-xs font-medium text-amber-900 mb-1">
                  Price Change Reason (Audit Log) *
                </label>
                <input
                  type="text"
                  required
                  placeholder="e.g. Annual rate indexation, renegotiated subcontractor margin"
                  value={editFormData.priceChangeReason}
                  onChange={(e) => setEditFormData({ ...editFormData, priceChangeReason: e.target.value })}
                  className="w-full px-3 py-2 border border-amber-300 rounded-lg text-sm bg-white focus:ring-2 focus:ring-amber-500"
                />
              </div>
            )}

            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Description</label>
              <textarea
                rows={3}
                value={editFormData.description}
                onChange={(e) => setEditFormData({ ...editFormData, description: e.target.value })}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="editIsActive"
                checked={editFormData.isActive}
                onChange={(e) => setEditFormData({ ...editFormData, isActive: e.target.checked })}
                className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
              />
              <label htmlFor="editIsActive" className="text-sm font-medium text-slate-700">
                Service is Active for Quotations
              </label>
            </div>

            <div className="flex justify-end gap-3 pt-3 border-t border-slate-200">
              <button
                type="button"
                onClick={() => setEditingService(null)}
                className="px-4 py-2 border border-slate-300 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium shadow-sm"
              >
                Save Changes
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Price History Modal */}
      {priceHistoryService && (
        <Modal
          isOpen={true}
          onClose={() => setPriceHistoryService(null)}
          title={`Price History - ${priceHistoryService.name} (${priceHistoryService.code})`}
        >
          <div className="space-y-4">
            <div className="text-xs text-slate-500">
              Complete historical rate movements and audit logs recorded for this service item.
            </div>

            <div className="max-h-80 overflow-y-auto">
              {priceHistories.length === 0 ? (
                <div className="text-center py-6 text-sm text-slate-400">
                  No previous price alterations recorded yet.
                </div>
              ) : (
                <div className="space-y-3">
                  {priceHistories.map((h) => (
                    <div key={h.id} className="p-3 bg-slate-50 border border-slate-200 rounded-lg text-sm space-y-1">
                      <div className="flex justify-between items-center">
                        <span className="font-semibold text-slate-900">
                          {formatCurrency(h.newPrice)}
                        </span>
                        <span className="text-xs text-slate-400">
                          {new Date(h.effectiveDate).toLocaleDateString()}
                        </span>
                      </div>
                      <div className="text-xs text-slate-500">
                        Previous: <span className="line-through">{formatCurrency(h.oldPrice)}</span>
                      </div>
                      {h.reason && (
                        <div className="text-xs italic text-slate-600 mt-1">
                          Reason: {h.reason}
                        </div>
                      )}
                      <div className="text-[11px] text-slate-400">
                        Modified by: {h.changedByName}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <div className="flex justify-end pt-3 border-t border-slate-200">
              <button
                type="button"
                onClick={() => setPriceHistoryService(null)}
                className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-sm font-medium"
              >
                Close
              </button>
            </div>
          </div>
        </Modal>
      )}

      {/* Manage Categories Modal */}
      <Modal
        isOpen={isCategoryModalOpen}
        onClose={() => setIsCategoryModalOpen(false)}
        title="Manage Service Categories"
      >
        <div className="space-y-4">
          <form onSubmit={handleCreateCategory} className="flex gap-2">
            <input
              type="text"
              required
              placeholder="CODE (e.g. CAT-DEV)"
              value={newCategoryCode}
              onChange={(e) => setNewCategoryCode(e.target.value)}
              className="w-1/3 px-3 py-2 border border-slate-300 rounded-lg text-sm font-mono focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              required
              placeholder="Category Name"
              value={newCategoryName}
              onChange={(e) => setNewCategoryName(e.target.value)}
              className="flex-1 px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
            />
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium text-sm rounded-lg"
            >
              Add
            </button>
          </form>

          <div className="border border-slate-200 rounded-lg divide-y divide-slate-200 max-h-64 overflow-y-auto">
            {categories.map((c) => (
              <div key={c.id} className="p-3 flex items-center justify-between hover:bg-slate-50 text-sm">
                <div>
                  <span className="font-mono text-xs font-semibold text-blue-600 mr-2">{c.code}</span>
                  <span className="font-medium text-slate-800">{c.name}</span>
                </div>
                <span className="text-xs text-slate-400">Index: {c.orderIndex}</span>
              </div>
            ))}
          </div>
        </div>
      </Modal>
    </div>
  );
};
