import React from 'react';
import { useParams, Link } from 'react-router-dom';
import {
  Printer,
  ArrowLeft,
  CheckCircle2,
} from 'lucide-react';

export const EmployeePayslipPage: React.FC = () => {
  const { id: _id } = useParams<{ id: string }>();

  // In production, an employee payslip view endpoint or record can be fetched
  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  return (
    <div className="min-h-screen bg-slate-100 p-4 sm:p-8 flex flex-col items-center">
      {/* Action buttons (hidden on print) */}
      <div className="max-w-3xl w-full flex items-center justify-between mb-4 print:hidden">
        <Link
          to="/payroll"
          className="inline-flex items-center gap-1.5 text-xs font-semibold text-slate-600 hover:text-slate-900"
        >
          <ArrowLeft className="w-4 h-4" />
          <span>Quay lại Bảng lương</span>
        </Link>
        <button
          onClick={() => window.print()}
          className="inline-flex items-center gap-1.5 px-4 py-2 text-xs font-semibold text-white bg-slate-900 rounded-lg hover:bg-slate-800 shadow-xs"
        >
          <Printer className="w-4 h-4" />
          <span>In phiếu lương (Print)</span>
        </button>
      </div>

      {/* Printable Payslip Card */}
      <div className="bg-white max-w-3xl w-full rounded-2xl border border-slate-200 shadow-md p-8 print:shadow-none print:border-none">
        {/* Header */}
        <div className="flex items-start justify-between pb-6 border-b border-slate-200">
          <div>
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 rounded-lg bg-blue-600 flex items-center justify-center text-white font-bold text-sm">
                CRM
              </div>
              <span className="font-bold text-lg text-slate-900">ENTERPRISE CRM CORP</span>
            </div>
            <p className="text-xs text-slate-500 mt-1">Hà Nội, Việt Nam • MST: 0109999888</p>
          </div>
          <div className="text-right">
            <span className="px-3 py-1 rounded-full text-xs font-bold bg-purple-100 text-purple-800">
              PHIẾU LƯƠNG & THU NHẬP
            </span>
            <p className="text-xs text-slate-400 mt-1">Kỳ lương Tháng 11/2026</p>
          </div>
        </div>

        {/* Employee Info */}
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 py-6 border-b border-slate-200 text-xs">
          <div>
            <span className="text-slate-400 block mb-0.5">Họ và tên nhân sự:</span>
            <span className="font-bold text-slate-900 text-sm">Nguyễn Văn An</span>
          </div>
          <div>
            <span className="text-slate-400 block mb-0.5">Mã nhân viên:</span>
            <span className="font-semibold text-slate-800">EMP-2026-001</span>
          </div>
          <div>
            <span className="text-slate-400 block mb-0.5">Phòng ban:</span>
            <span className="font-semibold text-slate-800">Kinh doanh B2B Enterprise</span>
          </div>
          <div>
            <span className="text-slate-400 block mb-0.5">Chức vụ:</span>
            <span className="font-semibold text-slate-800">Senior Account Executive</span>
          </div>
        </div>

        {/* Earnings Breakdown */}
        <div className="py-6 border-b border-slate-200">
          <h3 className="text-xs font-bold uppercase tracking-wider text-slate-400 mb-3">
            I. CÁC KHOẢN THU NHẬP (EARNINGS)
          </h3>
          <div className="space-y-2 text-xs">
            <div className="flex justify-between py-1.5 px-3 rounded-lg bg-slate-50">
              <span className="text-slate-700">1. Lương cơ bản theo hợp đồng:</span>
              <span className="font-semibold text-slate-900">{formatVnd(15000000)}</span>
            </div>
            <div className="flex justify-between py-1.5 px-3 rounded-lg bg-slate-50">
              <span className="text-slate-700">2. Hoa hồng bán hàng đã phê duyệt (Commissions):</span>
              <span className="font-bold text-emerald-600">{formatVnd(24500000)}</span>
            </div>
            <div className="flex justify-between py-1.5 px-3 rounded-lg bg-slate-50">
              <span className="text-slate-700">3. Tiền thưởng thành tích & vượt chỉ tiêu (Bonus):</span>
              <span className="font-bold text-amber-600">{formatVnd(5000000)}</span>
            </div>
            <div className="flex justify-between py-1.5 px-3 rounded-lg bg-slate-50">
              <span className="text-slate-700">4. Phụ cấp công tác & ăn trưa (Allowances):</span>
              <span className="font-semibold text-slate-900">{formatVnd(1500000)}</span>
            </div>
            <div className="flex justify-between py-2 px-3 rounded-lg bg-blue-50/60 font-bold text-slate-900">
              <span>TỔNG THU NHẬP TRƯỚC KHẤU TRỪ (GROSS):</span>
              <span>{formatVnd(46000000)}</span>
            </div>
          </div>
        </div>

        {/* Deductions Breakdown */}
        <div className="py-6 border-b border-slate-200">
          <h3 className="text-xs font-bold uppercase tracking-wider text-slate-400 mb-3">
            II. CÁC KHOẢN KHẤU TRỪ (DEDUCTIONS)
          </h3>
          <div className="space-y-2 text-xs">
            <div className="flex justify-between py-1.5 px-3 rounded-lg bg-slate-50">
              <span className="text-slate-700">1. Bảo hiểm xã hội & Y tế bắt buộc (10.5%):</span>
              <span className="font-medium text-rose-600">-{formatVnd(1575000)}</span>
            </div>
            <div className="flex justify-between py-1.5 px-3 rounded-lg bg-slate-50">
              <span className="text-slate-700">2. Thuế thu nhập cá nhân tạm tính (TNCN):</span>
              <span className="font-medium text-rose-600">-{formatVnd(2850000)}</span>
            </div>
            <div className="flex justify-between py-2 px-3 rounded-lg bg-rose-50/60 font-bold text-rose-800">
              <span>TỔNG CÁC KHOẢN KHẤU TRỪ:</span>
              <span>-{formatVnd(4425000)}</span>
            </div>
          </div>
        </div>

        {/* Net Pay Total Box */}
        <div className="my-6 p-5 rounded-2xl bg-gradient-to-r from-purple-50 to-indigo-50 border border-purple-200 flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold text-purple-700 uppercase tracking-wider">
              THỰC LĨNH CHUYỂN KHOẢN (NET SALARY)
            </span>
            <p className="text-xs text-slate-500 mt-0.5">Đã được phê duyệt bởi Ban Giám Đốc</p>
          </div>
          <div className="text-2xl font-extrabold text-purple-900">
            {formatVnd(41575000)}
          </div>
        </div>

        {/* Signatures */}
        <div className="grid grid-cols-3 gap-4 pt-6 text-center text-xs">
          <div>
            <p className="font-bold text-slate-700">Người lập phiếu</p>
            <p className="text-slate-400 mt-1">Kế toán tiền lương</p>
            <div className="h-16 flex items-center justify-center text-slate-300 italic text-[11px]">
              (Đã ký điện tử)
            </div>
          </div>
          <div>
            <p className="font-bold text-slate-700">Kế toán trưởng</p>
            <p className="text-slate-400 mt-1">Xác nhận số liệu</p>
            <div className="h-16 flex items-center justify-center text-slate-300 italic text-[11px]">
              (Đã ký điện tử)
            </div>
          </div>
          <div>
            <p className="font-bold text-slate-700">Giám đốc điều hành</p>
            <p className="text-slate-400 mt-1">Phê duyệt chi lương</p>
            <div className="h-16 flex items-center justify-center text-purple-600 font-bold text-xs">
              <CheckCircle2 className="w-5 h-5 mx-auto mb-1 text-emerald-600" />
              ĐÃ DUYỆT CHI TRẢ
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
