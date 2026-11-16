import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  FileText,
  Plus,
  ArrowLeft,
  Edit,
  Trash2,
  Eye,
  CheckCircle,
  XCircle,
  Info,
  Sparkles,
} from 'lucide-react';
import {
  contractTemplateService,
} from '../../services/contractTemplateService';
import type { ContractTemplate } from '../../types/billing';
import { Modal } from '../../components/common/Modal';

const AVAILABLE_TOKENS = [
  { token: '{{CustomerName}}', desc: 'Customer legal business entity name' },
  { token: '{{TaxId}}', desc: 'Enterprise registration / Tax Code' },
  { token: '{{Address}}', desc: 'Registered headquarters address' },
  { token: '{{ContactPerson}}', desc: 'Primary contact or legal representative' },
  { token: '{{ContractNumber}}', desc: 'Generated contract number (e.g. CTR-20261116-0001)' },
  { token: '{{TotalValue}}', desc: 'Total contract amount in formatted currency' },
  { token: '{{StartDate}}', desc: 'Contract effective start date' },
  { token: '{{EndDate}}', desc: 'Contract termination date' },
  { token: '{{ScopeOfWork}}', desc: 'Summary of contracted service deliverables' },
  { token: '{{PaymentTerms}}', desc: 'Commercial payment milestones' },
];

export const ContractTemplatesPage: React.FC = () => {
  const navigate = useNavigate();

  const [templates, setTemplates] = useState<ContractTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [previewTemplate, setPreviewTemplate] = useState<ContractTemplate | null>(null);

  // Form State
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [contentTemplate, setContentTemplate] = useState('');
  const [isActive, setIsActive] = useState(true);

  const loadTemplates = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await contractTemplateService.getTemplates(true);
      if (res.data) setTemplates(res.data);
    } catch (err) {
      console.error('Failed to load templates', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadTemplates();
  }, [loadTemplates]);

  const handleOpenCreate = () => {
    setEditingId(null);
    setCode('');
    setName('');
    setDescription('');
    setContentTemplate(
      `CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM\nĐộc lập - Tự do - Hạnh phúc\n-------------------\n\nHỢP ĐỒNG DỊCH VỤ SỐ: {{ContractNumber}}\n\nHôm nay, chúng tôi gồm có:\nBÊN A (Khách hàng): {{CustomerName}}\nMã số thuế: {{TaxId}}\nĐịa chỉ: {{Address}}\nĐại diện: {{ContactPerson}}\n\nBÊN B (Cung cấp dịch vụ): CÔNG TY CỔ PHẦN CÔNG NGHỆ ENTERPRISE\nMã số thuế: 0109988776\n\nĐIỀU 1: PHẠM VI CÔNG VIỆC\n{{ScopeOfWork}}\n\nĐIỀU 2: GIÁ TRỊ HỢP ĐỒNG VÀ PHƯƠNG THỨC THANH TOÁN\nTổng giá trị hợp đồng: {{TotalValue}}\nĐiều khoản thanh toán: {{PaymentTerms}}\n\nĐIỀU 3: THỜI HẠN THỰC HIỆN\nTừ ngày {{StartDate}} đến ngày {{EndDate}}.\n\nĐẠI DIỆN BÊN A                    ĐẠI DIỆN BÊN B`
    );
    setIsActive(true);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (tpl: ContractTemplate) => {
    setEditingId(tpl.id);
    setCode(tpl.code);
    setName(tpl.name);
    setDescription(tpl.description || '');
    setContentTemplate(tpl.contentTemplate);
    setIsActive(tpl.isActive);
    setIsModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await contractTemplateService.updateTemplate(editingId, {
          name,
          description,
          contentTemplate,
          isActive,
        });
      } else {
        await contractTemplateService.createTemplate({
          code: code.toUpperCase(),
          name,
          description,
          contentTemplate,
        });
      }
      setIsModalOpen(false);
      loadTemplates();
    } catch (err) {
      console.error('Failed to save template', err);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this template?')) return;
    try {
      await contractTemplateService.deleteTemplate(id);
      loadTemplates();
    } catch (err) {
      console.error('Failed to delete template', err);
    }
  };

  const insertToken = (token: string) => {
    setContentTemplate((prev) => prev + token);
  };

  const renderSamplePreview = (raw: string) => {
    return raw
      .replace(/{{CustomerName}}/g, 'Công ty Cổ phần Tập đoàn Viễn thông V-Tech')
      .replace(/{{TaxId}}/g, '0108876543')
      .replace(/{{Address}}/g, 'Tầng 18, Keangnam Landmark 72, Hà Nội')
      .replace(/{{ContactPerson}}/g, 'Ông Trần Văn Nam - Giám đốc Công nghệ')
      .replace(/{{ContractNumber}}/g, 'CTR-20261116-0001')
      .replace(/{{TotalValue}}/g, '144.100.000 ₫')
      .replace(/{{StartDate}}/g, '16/11/2026')
      .replace(/{{EndDate}}/g, '16/11/2027')
      .replace(/{{ScopeOfWork}}/g, 'Triển khai Kubernetes Production Cluster & 02 tháng Backend API Engineering.')
      .replace(/{{PaymentTerms}}/g, '50% tạm ứng khi ký kết, 50% sau khi nghiệm thu bàn giao.');
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <button
            onClick={() => navigate('/contracts')}
            className="flex items-center gap-2 text-xs text-slate-500 hover:text-slate-800 mb-2 font-medium"
          >
            <ArrowLeft className="w-3.5 h-3.5" /> Back to Contracts
          </button>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 flex items-center gap-2">
            <FileText className="w-7 h-7 text-indigo-600" />
            Configurable Contract Templates & Tokens
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Standardize legally binding agreement frameworks with dynamic customer token merge fields.
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white font-medium text-sm rounded-lg shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          Create Template
        </button>
      </div>

      {/* Templates Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {isLoading && templates.length === 0 ? (
          <div className="col-span-full py-12 text-center text-slate-400">Loading templates...</div>
        ) : templates.length === 0 ? (
          <div className="col-span-full py-12 text-center text-slate-400">
            No templates configured yet. Click "Create Template" to add one.
          </div>
        ) : (
          templates.map((tpl) => (
            <div
              key={tpl.id}
              className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs flex flex-col justify-between hover:shadow-md transition-shadow"
            >
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <span className="font-mono text-xs font-semibold px-2 py-0.5 bg-slate-100 text-indigo-700 rounded">
                    {tpl.code}
                  </span>
                  <span className="text-xs font-medium text-slate-400">v{tpl.version}</span>
                </div>

                <div>
                  <h3 className="font-bold text-slate-900 text-base">{tpl.name}</h3>
                  {tpl.description && (
                    <p className="text-xs text-slate-500 mt-1 line-clamp-2">{tpl.description}</p>
                  )}
                </div>

                <div className="bg-slate-50 p-3 rounded-lg border border-slate-100 text-xs text-slate-600 font-mono line-clamp-4 leading-relaxed">
                  {tpl.contentTemplate}
                </div>
              </div>

              <div className="mt-5 pt-3 border-t border-slate-100 flex items-center justify-between text-xs">
                <div>
                  {tpl.isActive ? (
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
                    onClick={() => setPreviewTemplate(tpl)}
                    className="p-1.5 text-slate-500 hover:text-indigo-600 rounded transition-colors"
                    title="Preview Merged Template"
                  >
                    <Eye className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => handleOpenEdit(tpl)}
                    className="p-1.5 text-slate-500 hover:text-blue-600 rounded transition-colors"
                    title="Edit Template"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => handleDelete(tpl.id)}
                    className="p-1.5 text-slate-500 hover:text-rose-600 rounded transition-colors"
                    title="Delete Template"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Create / Edit Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingId ? 'Edit Contract Template' : 'Create Contract Template'}
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Template Code *</label>
              <input
                type="text"
                required
                disabled={!!editingId}
                placeholder="e.g. TPL-MSA-2026"
                value={code}
                onChange={(e) => setCode(e.target.value.toUpperCase())}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm font-mono focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Template Name *</label>
              <input
                type="text"
                required
                placeholder="e.g. Master Services Agreement (MSA)"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Description</label>
            <input
              type="text"
              placeholder="e.g. Standard terms for enterprise IT and software consulting engagements"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          {/* Tokens insertion bar */}
          <div className="bg-indigo-50 border border-indigo-100 p-3 rounded-lg space-y-1.5">
            <div className="flex items-center gap-1.5 text-xs font-semibold text-indigo-900">
              <Sparkles className="w-3.5 h-3.5 text-indigo-600" /> Click to insert token merge field:
            </div>
            <div className="flex flex-wrap gap-1.5">
              {AVAILABLE_TOKENS.map((t) => (
                <button
                  key={t.token}
                  type="button"
                  onClick={() => insertToken(t.token)}
                  title={t.desc}
                  className="px-2 py-0.5 bg-white border border-indigo-200 text-indigo-700 font-mono text-[11px] rounded hover:bg-indigo-600 hover:text-white transition-colors"
                >
                  {t.token}
                </button>
              ))}
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Contract Body Template *</label>
            <textarea
              rows={12}
              required
              value={contentTemplate}
              onChange={(e) => setContentTemplate(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-xs font-mono focus:ring-2 focus:ring-indigo-500 leading-relaxed"
            />
          </div>

          {editingId && (
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="tplActive"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
                className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
              />
              <label htmlFor="tplActive" className="text-sm font-medium text-slate-700">
                Template is Active
              </label>
            </div>
          )}

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
              Save Template
            </button>
          </div>
        </form>
      </Modal>

      {/* Preview Modal */}
      {previewTemplate && (
        <Modal
          isOpen={true}
          onClose={() => setPreviewTemplate(null)}
          title={`Preview Template: ${previewTemplate.name} (${previewTemplate.code})`}
        >
          <div className="space-y-4">
            <div className="text-xs text-slate-500 flex items-center gap-1.5 bg-slate-50 p-2.5 rounded-lg border border-slate-200">
              <Info className="w-4 h-4 text-slate-400" />
              Showing simulated dynamic document output with sample customer and quotation token values merged.
            </div>

            <div className="max-h-96 overflow-y-auto p-4 bg-slate-50 border border-slate-200 rounded-lg font-serif text-xs text-slate-800 whitespace-pre-wrap leading-relaxed">
              {renderSamplePreview(previewTemplate.contentTemplate)}
            </div>

            <div className="flex justify-end pt-3 border-t border-slate-200">
              <button
                type="button"
                onClick={() => setPreviewTemplate(null)}
                className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-sm font-medium"
              >
                Close Preview
              </button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};
