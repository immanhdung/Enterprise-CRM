import React, { useState, useEffect, useCallback } from 'react';
import {
  Plus,
  Tag,
  Calendar,
  CheckCircle,
  XCircle,
  Star,
  Trash2,
  Edit,
} from 'lucide-react';
import { serviceCatalogService, type CreatePriceListRequest } from '../../services/serviceCatalogService';
import type { PriceList, ServiceItem } from '../../types/billing';
import { Modal } from '../../components/common/Modal';

export const PriceListsPage: React.FC = () => {
  const [priceLists, setPriceLists] = useState<PriceList[]>([]);
  const [services, setServices] = useState<ServiceItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  // Form State
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [currency, setCurrency] = useState('VND');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [isDefault, setIsDefault] = useState(false);
  const [isActive, setIsActive] = useState(true);
  const [items, setItems] = useState<
    Array<{
      serviceId: string;
      unitPrice: number;
      minQuantity: number;
      maxDiscountRate: number;
    }>
  >([]);

  const loadData = useCallback(async () => {
    setIsLoading(true);
    try {
      const [plRes, svcRes] = await Promise.all([
        serviceCatalogService.getPriceLists(true),
        serviceCatalogService.getServices({ isActive: true }),
      ]);
      if (plRes.data) setPriceLists(plRes.data);
      if (svcRes.data) setServices(svcRes.data);
    } catch (err) {
      console.error('Failed to load price lists', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleOpenCreate = () => {
    setEditingId(null);
    setCode('');
    setName('');
    setDescription('');
    setCurrency('VND');
    setStartDate('');
    setEndDate('');
    setIsDefault(false);
    setIsActive(true);
    setItems([]);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (pl: PriceList) => {
    setEditingId(pl.id);
    setCode(pl.code);
    setName(pl.name);
    setDescription(pl.description || '');
    setCurrency(pl.currency);
    setStartDate(pl.startDate ? pl.startDate.split('T')[0] : '');
    setEndDate(pl.endDate ? pl.endDate.split('T')[0] : '');
    setIsDefault(pl.isDefault);
    setIsActive(pl.isActive);
    setItems(
      pl.items.map((i) => ({
        serviceId: i.serviceId,
        unitPrice: i.unitPrice,
        minQuantity: i.minQuantity,
        maxDiscountRate: i.maxDiscountRate,
      }))
    );
    setIsModalOpen(true);
  };

  const handleAddItem = () => {
    if (services.length === 0) return;
    setItems([
      ...items,
      {
        serviceId: services[0].id,
        unitPrice: services[0].basePrice,
        minQuantity: 1,
        maxDiscountRate: 10,
      },
    ]);
  };

  const handleRemoveItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index));
  };

  const handleItemChange = (index: number, field: string, value: any) => {
    const updated = [...items];
    updated[index] = { ...updated[index], [field]: value };

    if (field === 'serviceId') {
      const matched = services.find((s) => s.id === value);
      if (matched) {
        updated[index].unitPrice = matched.basePrice;
      }
    }

    setItems(updated);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload: CreatePriceListRequest = {
      code: code.toUpperCase(),
      name,
      description,
      currency,
      startDate: startDate || undefined,
      endDate: endDate || undefined,
      isDefault,
      isActive,
      items,
    };

    try {
      if (editingId) {
        await serviceCatalogService.updatePriceList(editingId, payload);
      } else {
        await serviceCatalogService.createPriceList(payload);
      }
      setIsModalOpen(false);
      loadData();
    } catch (err) {
      console.error('Failed to save price list', err);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this price list?')) return;
    try {
      await serviceCatalogService.deletePriceList(id);
      loadData();
    } catch (err) {
      console.error('Failed to delete price list', err);
    }
  };

  const formatCurrency = (val: number, curr = 'VND') => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: curr }).format(val);
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 flex items-center gap-2">
            <Tag className="w-7 h-7 text-indigo-600" />
            Configurable Price Lists & Volume Tiers
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Maintain customer-specific rate cards, promotional campaigns, and volume discount brackets.
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white font-medium text-sm rounded-lg shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          New Price List
        </button>
      </div>

      {/* Grid of Price Lists */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {isLoading && priceLists.length === 0 ? (
          <div className="col-span-full py-12 text-center text-slate-400">Loading price lists...</div>
        ) : priceLists.length === 0 ? (
          <div className="col-span-full py-12 text-center text-slate-400">
            No price lists configured yet. Click "New Price List" to establish customized rate cards.
          </div>
        ) : (
          priceLists.map((pl) => (
            <div
              key={pl.id}
              className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs flex flex-col justify-between hover:shadow-md transition-shadow relative overflow-hidden"
            >
              {pl.isDefault && (
                <div className="absolute top-0 right-0 bg-amber-500 text-white text-[10px] font-bold px-2 py-0.5 rounded-bl-md flex items-center gap-1 shadow-xs">
                  <Star className="w-3 h-3 fill-current" /> DEFAULT
                </div>
              )}

              <div className="space-y-3">
                <div className="flex items-center gap-2">
                  <span className="font-mono text-xs font-semibold px-2 py-0.5 bg-slate-100 text-slate-700 rounded">
                    {pl.code}
                  </span>
                  <span className="text-xs font-semibold text-indigo-600 uppercase tracking-wider">
                    {pl.currency}
                  </span>
                </div>

                <div>
                  <h3 className="font-bold text-slate-900 text-base">{pl.name}</h3>
                  {pl.description && (
                    <p className="text-xs text-slate-500 mt-0.5 line-clamp-2">{pl.description}</p>
                  )}
                </div>

                <div className="text-xs text-slate-500 flex items-center gap-2">
                  <Calendar className="w-3.5 h-3.5 text-slate-400" />
                  {pl.startDate || pl.endDate ? (
                    <span>
                      {pl.startDate ? new Date(pl.startDate).toLocaleDateString() : 'Immediate'} -{' '}
                      {pl.endDate ? new Date(pl.endDate).toLocaleDateString() : 'Indefinite'}
                    </span>
                  ) : (
                    <span>Always Effective</span>
                  )}
                </div>

                {/* Items Summary */}
                <div className="bg-slate-50 p-3 rounded-lg border border-slate-100 text-xs space-y-1.5">
                  <div className="font-medium text-slate-700 flex justify-between">
                    <span>Configured Items</span>
                    <span className="text-indigo-600 font-semibold">{pl.items.length} services</span>
                  </div>
                  {pl.items.slice(0, 3).map((it) => (
                    <div key={it.id} className="flex justify-between text-slate-500">
                      <span className="truncate max-w-[150px]">{it.serviceName || it.serviceCode}</span>
                      <span className="font-mono font-medium text-slate-800">
                        {formatCurrency(it.unitPrice, pl.currency)}
                      </span>
                    </div>
                  ))}
                  {pl.items.length > 3 && (
                    <div className="text-slate-400 text-[11px] pt-0.5">
                      + {pl.items.length - 3} more items in rate card
                    </div>
                  )}
                </div>
              </div>

              {/* Footer */}
              <div className="mt-5 pt-3 border-t border-slate-100 flex items-center justify-between text-xs">
                <div>
                  {pl.isActive ? (
                    <span className="inline-flex items-center gap-1 text-emerald-600 font-medium">
                      <CheckCircle className="w-3.5 h-3.5" /> Active
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1 text-rose-500 font-medium">
                      <XCircle className="w-3.5 h-3.5" /> Inactive
                    </span>
                  )}
                </div>

                <div className="flex items-center gap-1">
                  <button
                    onClick={() => handleOpenEdit(pl)}
                    className="p-1.5 text-slate-500 hover:text-indigo-600 rounded transition-colors"
                    title="Edit Rate Card"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => handleDelete(pl.id)}
                    className="p-1.5 text-slate-500 hover:text-rose-600 rounded transition-colors"
                    title="Delete Rate Card"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Create / Edit Price List Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingId ? 'Edit Price List & Volume Tiers' : 'Create New Price List'}
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Price List Code *</label>
              <input
                type="text"
                required
                disabled={!!editingId}
                placeholder="e.g. PL-ENTERPRISE-2026"
                value={code}
                onChange={(e) => setCode(e.target.value.toUpperCase())}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm font-mono focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Currency *</label>
              <select
                value={currency}
                onChange={(e) => setCurrency(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              >
                <option value="VND">VND</option>
                <option value="USD">USD</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Price List Name *</label>
            <input
              type="text"
              required
              placeholder="e.g. VIP Strategic Partner Tariff"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Effective Start Date</label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Effective End Date</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>

          <div className="flex items-center gap-6">
            <label className="flex items-center gap-2 text-sm text-slate-700 cursor-pointer">
              <input
                type="checkbox"
                checked={isDefault}
                onChange={(e) => setIsDefault(e.target.checked)}
                className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
              />
              Default Price List for Quotations
            </label>

            <label className="flex items-center gap-2 text-sm text-slate-700 cursor-pointer">
              <input
                type="checkbox"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
                className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
              />
              Active
            </label>
          </div>

          {/* Line Items Builder */}
          <div className="space-y-3 pt-2 border-t border-slate-200">
            <div className="flex justify-between items-center">
              <h4 className="text-xs font-semibold uppercase text-slate-700 tracking-wider">
                Service Pricing Overrides & Volume Tiers
              </h4>
              <button
                type="button"
                onClick={handleAddItem}
                className="text-xs text-indigo-600 hover:text-indigo-800 font-medium flex items-center gap-1"
              >
                <Plus className="w-3.5 h-3.5" /> Add Tier
              </button>
            </div>

            <div className="space-y-2 max-h-52 overflow-y-auto pr-1">
              {items.map((item, idx) => (
                <div key={idx} className="flex items-center gap-2 bg-slate-50 p-2 rounded-lg border border-slate-200">
                  <select
                    value={item.serviceId}
                    onChange={(e) => handleItemChange(idx, 'serviceId', e.target.value)}
                    className="flex-2 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white"
                  >
                    {services.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.code} - {s.name}
                      </option>
                    ))}
                  </select>

                  <input
                    type="number"
                    placeholder="Unit Price"
                    value={item.unitPrice}
                    onChange={(e) => handleItemChange(idx, 'unitPrice', Number(e.target.value))}
                    className="w-28 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white text-right"
                  />

                  <input
                    type="number"
                    min="1"
                    title="Min Quantity"
                    placeholder="Min Qty"
                    value={item.minQuantity}
                    onChange={(e) => handleItemChange(idx, 'minQuantity', Number(e.target.value))}
                    className="w-18 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white text-center"
                  />

                  <input
                    type="number"
                    min="0"
                    max="100"
                    title="Max Discount %"
                    placeholder="Max Disc%"
                    value={item.maxDiscountRate}
                    onChange={(e) => handleItemChange(idx, 'maxDiscountRate', Number(e.target.value))}
                    className="w-20 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white text-center"
                  />

                  <button
                    type="button"
                    onClick={() => handleRemoveItem(idx)}
                    className="p-1 text-slate-400 hover:text-rose-500"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              ))}
              {items.length === 0 && (
                <div className="text-center py-4 text-xs text-slate-400 italic">
                  No service overrides defined. All services will default to catalog base price.
                </div>
              )}
            </div>
          </div>

          <div className="flex justify-end gap-3 pt-3 border-t border-slate-200">
            <button
              type="button"
              onClick={() => setIsModalOpen(false)}
              className="px-4 py-2 border border-slate-300 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-medium shadow-sm"
            >
              Save Price List
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
