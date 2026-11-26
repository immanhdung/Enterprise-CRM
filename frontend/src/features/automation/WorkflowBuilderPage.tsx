import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { AutomationNavigation } from './AutomationNavigation';
import { automationService } from '../../services/automationService';
import type { CreateWorkflowRequest } from '../../types/automation';
import { ArrowLeft, Save, Plus, Trash2, Zap, Sliders, PlayCircle } from 'lucide-react';

export const WorkflowBuilderPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('Lead');
  const [triggerEvent, setTriggerEvent] = useState('LeadCreated');
  const [executionOrder, setExecutionOrder] = useState(1);
  const [isActive, setIsActive] = useState(true);

  // Conditions
  interface ConditionItem {
    field: string;
    operator: string;
    value: string;
  }
  const [conditions, setConditions] = useState<ConditionItem[]>([
    { field: 'Source', operator: 'Equals', value: 'WebsiteForm' },
  ]);

  // Actions
  interface ActionItem {
    type: string;
    name: string;
    title?: string;
    message?: string;
    taskTitle?: string;
  }
  const [actions, setActions] = useState<ActionItem[]>([
    { type: 'SendNotification', name: 'Gửi thông báo Sales', title: 'Lead mới từ Website', message: 'Hệ thống vừa tiếp nhận lead mới' },
  ]);

  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (id) {
      automationService.getWorkflowById(id).then((w) => {
        if (w) {
          setName(w.name);
          setDescription(w.description);
          setCategory(w.category);
          setTriggerEvent(w.triggerEvent);
          setExecutionOrder(w.executionOrder);
          setIsActive(w.isActive);
          try {
            if (w.conditionsJson) setConditions(JSON.parse(w.conditionsJson));
            if (w.actionsJson) setActions(JSON.parse(w.actionsJson));
          } catch (e) {
            console.error('Failed to parse json', e);
          }
        }
      });
    }
  }, [id]);

  const addCondition = () => {
    setConditions([...conditions, { field: 'Amount', operator: 'GreaterThan', value: '50000000' }]);
  };

  const removeCondition = (index: number) => {
    setConditions(conditions.filter((_, i) => i !== index));
  };

  const updateCondition = (index: number, key: keyof ConditionItem, val: string) => {
    const updated = [...conditions];
    updated[index][key] = val;
    setConditions(updated);
  };

  const addAction = () => {
    setActions([...actions, { type: 'CreateTask', name: 'Tạo tác vụ theo dõi', taskTitle: 'Liên hệ khách hàng trong 24h' }]);
  };

  const removeAction = (index: number) => {
    setActions(actions.filter((_, i) => i !== index));
  };

  const updateAction = (index: number, key: keyof ActionItem, val: string) => {
    const updated = [...actions];
    updated[index][key] = val;
    setActions(updated);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      alert('Vui lòng nhập tên quy trình');
      return;
    }

    const payload: CreateWorkflowRequest = {
      name: name.trim(),
      description: description.trim(),
      category,
      triggerEvent,
      executionOrder,
      isActive,
      conditionsJson: JSON.stringify(conditions),
      actionsJson: JSON.stringify(actions),
    };

    try {
      setSaving(true);
      if (isEdit && id) {
        await automationService.updateWorkflow(id, payload);
      } else {
        await automationService.createWorkflow(payload);
      }
      navigate('/automation/workflows');
    } catch (err: any) {
      alert(`Lỗi: ${err?.message || 'Không thể lưu quy trình'}`);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-3">
          <Link to="/automation/workflows" className="p-2 text-gray-500 hover:text-gray-700 bg-white rounded-lg border border-gray-200">
            <ArrowLeft className="w-5 h-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              {isEdit ? 'Chỉnh sửa Quy trình Tự động' : 'Thiết kế Quy trình Tự động Mới'}
            </h1>
            <p className="text-sm text-gray-500">Xây dựng chuỗi kích hoạt, điều kiện và hành động kinh doanh</p>
          </div>
        </div>
        <button
          onClick={handleSave}
          disabled={saving}
          className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2 rounded-lg font-medium shadow-sm transition-colors text-sm"
        >
          <Save className="w-4 h-4" />
          <span>{saving ? 'Đang lưu...' : 'Lưu Quy trình'}</span>
        </button>
      </div>

      <AutomationNavigation />

      <form onSubmit={handleSave} className="space-y-6">
        {/* Step 1: Basic Information */}
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
          <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <Sliders className="w-5 h-5 text-blue-600" />
            1. Thông tin cơ bản
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Tên quy trình *</label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="VD: Tự động phân bổ Lead VIP và gửi thông báo"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Phân loại</label>
              <select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
              >
                <option value="Lead">Lead & Khách hàng tiềm năng</option>
                <option value="Deal">Cơ hội bán hàng (Deals)</option>
                <option value="Contract">Hợp đồng & E-Signature</option>
                <option value="Finance">Tài chính & Thanh toán</option>
                <option value="System">Hệ thống chung</option>
              </select>
            </div>
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Mô tả mục đích</label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={2}
                placeholder="Mô tả tóm tắt kịch bản nghiệp vụ khi nào kích hoạt và xử lý ra sao..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Thứ tự ưu tiên thực thi</label>
              <input
                type="number"
                min={1}
                value={executionOrder}
                onChange={(e) => setExecutionOrder(parseInt(e.target.value) || 1)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
              />
            </div>
            <div className="flex items-center gap-3 pt-6">
              <input
                type="checkbox"
                id="isActiveCheck"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
                className="w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
              />
              <label htmlFor="isActiveCheck" className="text-sm font-medium text-gray-900 cursor-pointer">
                Kích hoạt quy trình ngay sau khi lưu
              </label>
            </div>
          </div>
        </div>

        {/* Step 2: Trigger Event */}
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
          <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <Zap className="w-5 h-5 text-amber-500" />
            2. Sự kiện kích hoạt (Trigger Event)
          </h2>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Chọn sự kiện khởi phát quy trình *</label>
            <select
              value={triggerEvent}
              onChange={(e) => setTriggerEvent(e.target.value)}
              className="w-full md:w-1/2 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none font-medium text-gray-800"
            >
              <option value="LeadCreated">LeadCreated – Khi có Khách hàng tiềm năng mới</option>
              <option value="LeadStatusChanged">LeadStatusChanged – Khi trạng thái Lead thay đổi</option>
              <option value="DealWon">DealWon – Khi cơ hội bán hàng chuyển thành công (Won)</option>
              <option value="DealLost">DealLost – Khi cơ hội thất bại (Lost)</option>
              <option value="ContractSigned">ContractSigned – Khi hợp đồng hoàn tất ký điện tử</option>
              <option value="PaymentReceived">PaymentReceived – Khi ghi nhận thanh toán tiền mặt/chuyển khoản</option>
              <option value="PaymentOverdue">PaymentOverdue – Khi đợt thanh toán bị quá hạn</option>
              <option value="KpiEvaluated">KpiEvaluated – Khi hoàn tất đánh giá KPI nhân sự</option>
              <option value="ManualTrigger">ManualTrigger – Kích hoạt thủ công</option>
            </select>
          </div>
        </div>

        {/* Step 3: Conditions Builder */}
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-base font-semibold text-gray-900 flex items-center gap-2">
              <Sliders className="w-5 h-5 text-purple-600" />
              3. Điều kiện lọc dữ liệu (Conditions)
            </h2>
            <button
              type="button"
              onClick={addCondition}
              className="inline-flex items-center gap-1 text-xs text-blue-600 hover:text-blue-800 font-medium px-2.5 py-1.5 bg-blue-50 rounded border border-blue-200"
            >
              <Plus className="w-3.5 h-3.5" /> Thêm điều kiện
            </button>
          </div>

          {conditions.length === 0 ? (
            <p className="text-sm text-gray-500 italic">Không có điều kiện lọc nào (quy trình sẽ kích hoạt cho mọi bản ghi).</p>
          ) : (
            <div className="space-y-3">
              {conditions.map((cond, idx) => (
                <div key={idx} className="flex items-center gap-3 bg-gray-50 p-3 rounded-lg border border-gray-200 text-sm">
                  <div className="w-1/3">
                    <input
                      type="text"
                      placeholder="Trường (Field: Amount, Source...)"
                      value={cond.field}
                      onChange={(e) => updateCondition(idx, 'field', e.target.value)}
                      className="w-full px-3 py-1.5 border border-gray-300 rounded text-sm bg-white"
                    />
                  </div>
                  <div className="w-1/4">
                    <select
                      value={cond.operator}
                      onChange={(e) => updateCondition(idx, 'operator', e.target.value)}
                      className="w-full px-3 py-1.5 border border-gray-300 rounded text-sm bg-white"
                    >
                      <option value="Equals">Bằng (Equals)</option>
                      <option value="NotEquals">Không bằng (NotEquals)</option>
                      <option value="Contains">Chứa ký tự (Contains)</option>
                      <option value="GreaterThan">Lớn hơn (&gt;)</option>
                      <option value="LessThan">Nhỏ hơn (&lt;)</option>
                    </select>
                  </div>
                  <div className="flex-1">
                    <input
                      type="text"
                      placeholder="Giá trị kỳ vọng"
                      value={cond.value}
                      onChange={(e) => updateCondition(idx, 'value', e.target.value)}
                      className="w-full px-3 py-1.5 border border-gray-300 rounded text-sm bg-white"
                    />
                  </div>
                  <button
                    type="button"
                    onClick={() => removeCondition(idx)}
                    className="p-1.5 text-red-500 hover:text-red-700"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Step 4: Actions Builder */}
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-base font-semibold text-gray-900 flex items-center gap-2">
              <PlayCircle className="w-5 h-5 text-green-600" />
              4. Các hành động thực thi (Actions)
            </h2>
            <button
              type="button"
              onClick={addAction}
              className="inline-flex items-center gap-1 text-xs text-green-600 hover:text-green-800 font-medium px-2.5 py-1.5 bg-green-50 rounded border border-green-200"
            >
              <Plus className="w-3.5 h-3.5" /> Thêm hành động
            </button>
          </div>

          {actions.length === 0 ? (
            <p className="text-sm text-gray-500 italic">Chưa thiết lập hành động nào cho quy trình này.</p>
          ) : (
            <div className="space-y-4">
              {actions.map((action, idx) => (
                <div key={idx} className="bg-gray-50 p-4 rounded-lg border border-gray-200 space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <span className="w-6 h-6 rounded-full bg-blue-600 text-white text-xs font-bold flex items-center justify-center">
                        {idx + 1}
                      </span>
                      <select
                        value={action.type}
                        onChange={(e) => updateAction(idx, 'type', e.target.value)}
                        className="px-3 py-1.5 border border-gray-300 rounded text-sm font-medium bg-white"
                      >
                        <option value="SendNotification">SendNotification – Gửi thông báo trong ứng dụng</option>
                        <option value="CreateTask">CreateTask – Tự động tạo công việc follow-up</option>
                        <option value="AssignLead">AssignLead – Phân bổ Lead tự động</option>
                        <option value="SendEmail">SendEmail – Gửi email thông báo</option>
                      </select>
                    </div>
                    <button
                      type="button"
                      onClick={() => removeAction(idx)}
                      className="text-red-500 hover:text-red-700 p-1"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>

                  {action.type === 'SendNotification' && (
                    <div className="grid grid-cols-2 gap-3 text-sm">
                      <div>
                        <label className="block text-xs font-medium text-gray-600 mb-1">Tiêu đề thông báo</label>
                        <input
                          type="text"
                          value={action.title || ''}
                          onChange={(e) => updateAction(idx, 'title', e.target.value)}
                          placeholder="VD: Cảnh báo Lead mới"
                          className="w-full px-3 py-1.5 border border-gray-300 rounded bg-white"
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-gray-600 mb-1">Nội dung thông báo</label>
                        <input
                          type="text"
                          value={action.message || ''}
                          onChange={(e) => updateAction(idx, 'message', e.target.value)}
                          placeholder="VD: Khách hàng yêu cầu liên hệ gấp"
                          className="w-full px-3 py-1.5 border border-gray-300 rounded bg-white"
                        />
                      </div>
                    </div>
                  )}

                  {action.type === 'CreateTask' && (
                    <div className="text-sm">
                      <label className="block text-xs font-medium text-gray-600 mb-1">Tiêu đề tác vụ</label>
                      <input
                        type="text"
                        value={action.taskTitle || ''}
                        onChange={(e) => updateAction(idx, 'taskTitle', e.target.value)}
                        placeholder="VD: Gọi điện tư vấn giải pháp CRM trong 24h"
                        className="w-full px-3 py-1.5 border border-gray-300 rounded bg-white"
                      />
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </form>
    </div>
  );
};
