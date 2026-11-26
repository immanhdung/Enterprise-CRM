import React, { useState, useEffect } from 'react';
import { aiService } from '../../services/aiService';
import type { GeneratedReport, ManagementReportData, ScheduledReport } from '../../types/ai';
import { FileText, Calendar, Plus, RefreshCw, Award, Clock } from 'lucide-react';

export const ReportsDashboardPage: React.FC = () => {
  const [reportType, setReportType] = useState<'Daily' | 'Weekly' | 'Monthly'>('Monthly');
  const [reportData, setReportData] = useState<ManagementReportData | null>(null);
  const [generatedReports, setGeneratedReports] = useState<GeneratedReport[]>([]);
  const [scheduledReports, setScheduledReports] = useState<ScheduledReport[]>([]);
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);

  const fetchReportsData = async () => {
    try {
      setLoading(true);
      const now = new Date();
      let start: Date;
      const end = now;

      if (reportType === 'Daily') {
        start = new Date(now.getFullYear(), now.getMonth(), now.getDate());
      } else if (reportType === 'Weekly') {
        const day = now.getDay() || 7;
        start = new Date(now.getFullYear(), now.getMonth(), now.getDate() - day + 1);
      } else {
        start = new Date(now.getFullYear(), now.getMonth(), 1);
      }

      const [data, genList, schList] = await Promise.all([
        aiService.getReportData(reportType, start.toISOString(), end.toISOString()),
        aiService.getGeneratedReports(reportType, 10),
        aiService.getScheduledReports(),
      ]);

      setReportData(data);
      setGeneratedReports(genList);
      setScheduledReports(schList);
    } catch (err) {
      console.error('Failed to load reports:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReportsData();
  }, [reportType]);

  const handleGenerateAdHoc = async () => {
    try {
      setGenerating(true);
      await aiService.generateReport(reportType);
      alert(`Đã kết xuất báo cáo quản trị ${reportType} thành công.`);
      fetchReportsData();
    } catch (err: any) {
      alert(`Lỗi tạo báo cáo: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setGenerating(false);
    }
  };

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Báo cáo Quản trị Doanh nghiệp (Management Reporting)</h1>
          <p className="text-sm text-gray-500">
            Hệ thống tổng hợp và tự động kết xuất báo cáo định kỳ: Ngày, Tuần, Tháng từ cơ sở dữ liệu thực tế
          </p>
        </div>
        <button
          onClick={handleGenerateAdHoc}
          disabled={generating}
          className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-medium shadow-sm transition-colors"
        >
          <Plus className={`w-4 h-4 ${generating ? 'animate-spin' : ''}`} />
          <span>{generating ? 'Đang tổng hợp...' : 'Kết xuất Báo cáo Mới'}</span>
        </button>
      </div>

      {/* Report Type Selector Tabs */}
      <div className="flex gap-3 mb-6">
        {(['Daily', 'Weekly', 'Monthly'] as const).map((type) => (
          <button
            key={type}
            onClick={() => setReportType(type)}
            className={`px-5 py-2.5 rounded-lg text-sm font-semibold transition-colors flex items-center gap-2 ${
              reportType === type
                ? 'bg-blue-600 text-white shadow-sm'
                : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-200'
            }`}
          >
            <Calendar className="w-4 h-4" />
            <span>Báo cáo {type === 'Daily' ? 'Hàng Ngày' : type === 'Weekly' ? 'Hàng Tuần' : 'Hàng Tháng'}</span>
          </button>
        ))}
      </div>

      {loading ? (
        <div className="flex justify-center items-center h-48 bg-white rounded-lg border border-gray-200">
          <RefreshCw className="w-6 h-6 animate-spin text-blue-600" />
        </div>
      ) : reportData ? (
        <div className="space-y-6">
          {/* Summary Financial Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
              <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider block">Doanh thu Hợp đồng mới</span>
              <div className="text-2xl font-bold text-gray-900 mt-2">
                {reportData.totalRevenue.toLocaleString('vi-VN')} <span className="text-sm font-normal text-gray-500">VND</span>
              </div>
              <div className="text-xs text-gray-500 mt-1">Từ các hợp đồng phát sinh trong kỳ</div>
            </div>

            <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
              <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider block">Dòng tiền Thực thu</span>
              <div className="text-2xl font-bold text-green-700 mt-2">
                {reportData.totalCollectedCash.toLocaleString('vi-VN')} <span className="text-sm font-normal text-gray-500">VND</span>
              </div>
              <div className="text-xs text-gray-500 mt-1">Tổng tiền mặt/chuyển khoản đã xác nhận</div>
            </div>

            <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
              <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider block">Công nợ Phải thu</span>
              <div className="text-2xl font-bold text-amber-600 mt-2">
                {reportData.totalReceivables.toLocaleString('vi-VN')} <span className="text-sm font-normal text-gray-500">VND</span>
              </div>
              <div className="text-xs text-gray-500 mt-1">Đợt thanh toán đến hạn và quá hạn</div>
            </div>
          </div>

          {/* Operational Metrics */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="bg-white p-4 rounded-lg border border-gray-200 shadow-sm text-center">
              <div className="text-2xl font-bold text-purple-700">{reportData.newLeadsCount}</div>
              <div className="text-xs text-gray-500 mt-1">Khách tiềm năng mới</div>
            </div>
            <div className="bg-white p-4 rounded-lg border border-gray-200 shadow-sm text-center">
              <div className="text-2xl font-bold text-blue-700">{reportData.dealsWonCount}</div>
              <div className="text-xs text-gray-500 mt-1">Thương vụ chốt thành công</div>
            </div>
            <div className="bg-white p-4 rounded-lg border border-gray-200 shadow-sm text-center">
              <div className="text-2xl font-bold text-indigo-700">{reportData.activeContractsCount}</div>
              <div className="text-xs text-gray-500 mt-1">Hợp đồng đang thực hiện</div>
            </div>
            <div className="bg-white p-4 rounded-lg border border-gray-200 shadow-sm text-center">
              <div className="text-2xl font-bold text-gray-800">
                {reportData.totalPayrollPaid.toLocaleString('vi-VN')}
              </div>
              <div className="text-xs text-gray-500 mt-1">Lương thưởng đã chi (VND)</div>
            </div>
          </div>

          {/* Top Performers Leaderboard */}
          <div className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200 bg-gray-50 flex items-center justify-between">
              <h3 className="text-sm font-bold text-gray-900 flex items-center gap-2">
                <Award className="w-4 h-4 text-amber-500" />
                Bảng vàng Vinh danh Sales (Top Performers)
              </h3>
              <span className="text-xs text-gray-500">Kỳ báo cáo: {reportData.periodLabel}</span>
            </div>

            {reportData.topPerformers.length === 0 ? (
              <div className="p-6 text-center text-xs text-gray-400 italic">Chưa có giao dịch chốt trong kỳ này.</div>
            ) : (
              <table className="min-w-full divide-y divide-gray-200 text-sm">
                <thead className="bg-gray-50 text-gray-600 font-semibold">
                  <tr>
                    <th className="px-6 py-3 text-left">Xếp hạng</th>
                    <th className="px-6 py-3 text-left">Nhân viên Sales</th>
                    <th className="px-6 py-3 text-left">Đội ngũ</th>
                    <th className="px-6 py-3 text-right">Doanh thu chốt (VND)</th>
                    <th className="px-6 py-3 text-center">Số Deal Won</th>
                    <th className="px-6 py-3 text-right">Tỷ lệ KPI</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {reportData.topPerformers.map((rep, idx) => (
                    <tr key={idx} className="hover:bg-gray-50">
                      <td className="px-6 py-3 font-bold text-gray-700">#{idx + 1}</td>
                      <td className="px-6 py-3 font-semibold text-gray-900">{rep.employeeName}</td>
                      <td className="px-6 py-3 text-gray-500">{rep.teamName}</td>
                      <td className="px-6 py-3 font-bold text-green-700 text-right">
                        {rep.achievedRevenue.toLocaleString('vi-VN')}
                      </td>
                      <td className="px-6 py-3 text-center font-medium">{rep.wonDeals}</td>
                      <td className="px-6 py-3 text-right font-semibold text-blue-600">
                        {rep.kpiAchievementRate}%
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* Generated Reports Archive */}
          <div className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200 bg-gray-50 flex items-center justify-between">
              <h3 className="text-sm font-bold text-gray-900 flex items-center gap-2">
                <Clock className="w-4 h-4 text-purple-600" />
                Lịch Báo Cáo Tự Động Đã Thiết Lập ({scheduledReports.length})
              </h3>
            </div>
            {scheduledReports.length === 0 ? (
              <div className="p-4 text-center text-xs text-gray-400 italic">Chưa có lịch báo cáo tự động nào.</div>
            ) : (
              <div className="divide-y divide-gray-200">
                {scheduledReports.map((sch) => (
                  <div key={sch.id} className="p-3.5 flex items-center justify-between hover:bg-gray-50 text-xs">
                    <div>
                      <span className="font-semibold text-gray-900">{sch.name}</span>
                      <span className="ml-2 text-gray-500">({sch.reportType} lúc {sch.timeOfDay})</span>
                    </div>
                    <span className={`px-2 py-0.5 rounded text-[11px] font-medium ${sch.isActive ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-gray-100 text-gray-600'}`}>
                      {sch.isActive ? 'Đang hoạt động' : 'Tạm dừng'}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Generated Reports Archive */}
          <div className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
              <h3 className="text-sm font-bold text-gray-900 flex items-center gap-2">
                <FileText className="w-4 h-4 text-blue-600" />
                Lịch sử các Báo cáo đã Kết xuất (Generated Reports Archive)
              </h3>
            </div>
            {generatedReports.length === 0 ? (
              <div className="p-6 text-center text-xs text-gray-400 italic">Chưa có bản báo cáo nào được lưu trữ.</div>
            ) : (
              <div className="divide-y divide-gray-200">
                {generatedReports.map((gen) => (
                  <div key={gen.id} className="p-4 flex items-center justify-between hover:bg-gray-50 text-xs">
                    <div>
                      <div className="font-semibold text-gray-900 text-sm">{gen.title}</div>
                      <div className="text-gray-500 mt-1">
                        Thời gian tạo: {new Date(gen.generatedAt).toLocaleString('vi-VN')}
                      </div>
                    </div>
                    <span className="px-2.5 py-1 bg-green-50 text-green-700 border border-green-200 rounded font-semibold">
                      Đã lưu trữ
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      ) : null}
    </div>
  );
};
