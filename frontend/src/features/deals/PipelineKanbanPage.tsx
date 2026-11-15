import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  DndContext,
  DragOverlay,
  useSensor,
  useSensors,
  PointerSensor,
  closestCorners,
  type DragStartEvent,
  type DragEndEvent,
  useDroppable,
  useDraggable,
} from '@dnd-kit/core';
import {
  Plus,
  Building2,
  Calendar,
  AlertCircle,
  CheckCircle,
  RefreshCw,
  Kanban,
  User,
} from 'lucide-react';
import { dealService } from '../../services/dealService';
import { employeeService, type EmployeeSimpleDto } from '../../services/employeeService';
import { CreateDealModal } from './components/CreateDealModal';
import type { KanbanBoardDto, KanbanDealCardDto, KanbanStageColumnDto } from '../../types/crm';

// Draggable Deal Card Component
const KanbanDealCard: React.FC<{
  deal: KanbanDealCardDto;
  isOverlay?: boolean;
}> = ({ deal, isOverlay = false }) => {
  const navigate = useNavigate();
  const { attributes, listeners, setNodeRef, isDragging } = useDraggable({
    id: deal.id,
    data: { deal },
  });

  return (
    <div
      ref={setNodeRef}
      {...attributes}
      {...listeners}
      className={`p-3.5 rounded-xl border bg-white dark:bg-slate-800 shadow-xs cursor-grab active:cursor-grabbing select-none transition-all ${
        isDragging ? 'opacity-30 border-blue-400' : 'hover:border-blue-300 dark:hover:border-blue-700'
      } ${
        isOverlay ? 'shadow-2xl border-blue-500 scale-105 rotate-1 ring-2 ring-blue-500/20' : 'border-slate-200 dark:border-slate-700'
      }`}
    >
      <div className="flex items-start justify-between gap-2 mb-1.5">
        <button
          onClick={(e) => {
            e.stopPropagation();
            navigate(`/deals/${deal.id}`);
          }}
          className="font-semibold text-xs text-slate-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400 text-left line-clamp-1"
        >
          {deal.title}
        </button>
        <span className="text-[10px] font-mono text-slate-400 shrink-0">{deal.code}</span>
      </div>

      <div className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400 mb-2.5">
        <Building2 className="w-3 h-3 text-blue-500 shrink-0" />
        <span className="truncate">{deal.accountName}</span>
      </div>

      <div className="p-2 rounded-lg bg-slate-50 dark:bg-slate-900/50 border border-slate-100 dark:border-slate-800 flex items-center justify-between text-xs mb-2.5">
        <div>
          <div className="text-[10px] text-slate-400">Total Value</div>
          <div className="font-bold text-slate-900 dark:text-white">
            ${Number(deal.amount).toLocaleString()}
          </div>
        </div>
        <div className="text-right">
          <div className="text-[10px] text-slate-400">Expected ({deal.probability}%)</div>
          <div className="font-semibold text-emerald-600 dark:text-emerald-400">
            ${Number(deal.expectedRevenue).toLocaleString()}
          </div>
        </div>
      </div>

      <div className="flex items-center justify-between text-[11px] text-slate-400 pt-1 border-t border-slate-100 dark:border-slate-700/60">
        <div className="flex items-center gap-1 truncate max-w-[130px]">
          <User className="w-3 h-3 shrink-0" />
          <span className="truncate">{deal.assignedEmployeeName || 'Unassigned'}</span>
        </div>
        {deal.expectedClosingDate && (
          <div className="flex items-center gap-1 shrink-0">
            <Calendar className="w-3 h-3" />
            <span>{dayjs(deal.expectedClosingDate).format('MMM D')}</span>
          </div>
        )}
      </div>
    </div>
  );
};

// Droppable Stage Column Component
const KanbanStageColumn: React.FC<{
  column: KanbanStageColumnDto;
  onAddDeal: (stageId: string) => void;
}> = ({ column, onAddDeal }) => {
  const { setNodeRef, isOver } = useDroppable({
    id: column.stageId,
    data: { column },
  });

  return (
    <div
      ref={setNodeRef}
      className={`flex flex-col w-72 shrink-0 rounded-2xl border transition-all ${
        isOver
          ? 'bg-blue-50/70 border-blue-400 dark:bg-blue-950/30 dark:border-blue-700 ring-2 ring-blue-500/20'
          : 'bg-slate-50/80 dark:bg-slate-900/60 border-slate-200 dark:border-slate-800'
      }`}
    >
      {/* Column Header */}
      <div className="p-3.5 border-b border-slate-200/80 dark:border-slate-800">
        <div className="flex items-center justify-between mb-2">
          <div className="flex items-center gap-2">
            <span
              className="w-2.5 h-2.5 rounded-full"
              style={{ backgroundColor: column.colorCode || '#3b82f6' }}
            />
            <h3 className="font-bold text-xs text-slate-800 dark:text-slate-100">
              {column.stageName}
            </h3>
            <span className="text-[11px] font-semibold px-2 py-0.5 rounded-full bg-slate-200 dark:bg-slate-800 text-slate-600 dark:text-slate-400">
              {column.dealsCount}
            </span>
          </div>

          <button
            onClick={() => onAddDeal(column.stageId)}
            title="Add Deal to Stage"
            className="p-1 rounded-md text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 hover:bg-slate-200 dark:hover:bg-slate-800 transition-colors"
          >
            <Plus className="w-3.5 h-3.5" />
          </button>
        </div>

        {/* Column Metrics */}
        <div className="flex items-center justify-between text-[11px] text-slate-500">
          <div>
            Total: <span className="font-semibold text-slate-800 dark:text-slate-200">${Number(column.totalAmount).toLocaleString()}</span>
          </div>
          <div>
            Exp: <span className="font-semibold text-emerald-600 dark:text-emerald-400">${Number(column.totalExpectedRevenue).toLocaleString()}</span>
          </div>
        </div>
      </div>

      {/* Cards List Droppable Area */}
      <div className="flex-1 p-2.5 space-y-2.5 min-h-[480px] overflow-y-auto max-h-[calc(100vh-280px)]">
        {column.deals.length === 0 ? (
          <div className="h-32 border-2 border-dashed border-slate-200 dark:border-slate-800 rounded-xl flex items-center justify-center text-xs text-slate-400">
            Drop deal here
          </div>
        ) : (
          column.deals.map((deal) => <KanbanDealCard key={deal.id} deal={deal} />)
        )}
      </div>
    </div>
  );
};

export const PipelineKanbanPage: React.FC = () => {
  const navigate = useNavigate();

  const [board, setBoard] = useState<KanbanBoardDto | null>(null);
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [notification, setNotification] = useState<string | null>(null);

  // Filters
  const [assignedEmployeeId, setAssignedEmployeeId] = useState('');
  const [status, setStatus] = useState('');

  // Drag state
  const [activeDeal, setActiveDeal] = useState<KanbanDealCardDto | null>(null);

  // Modal
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [targetStageId, setTargetStageId] = useState<string | undefined>(undefined);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 5, // 5px movement required to prevent accidental drag
      },
    })
  );

  const loadBoard = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await dealService.getKanbanBoard({
        assignedEmployeeId: assignedEmployeeId || undefined,
        status: status || undefined,
      });

      if (res.success && res.data) {
        setBoard(res.data);
      } else {
        setError(res.message || 'Failed to load pipeline board.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error fetching Kanban board.');
    } finally {
      setIsLoading(false);
    }
  }, [assignedEmployeeId, status]);

  useEffect(() => {
    loadBoard();
  }, [loadBoard]);

  useEffect(() => {
    employeeService.getEmployees().then((res) => {
      if (res.data) setEmployees(res.data);
    }).catch(console.error);
  }, []);

  const handleDragStart = (event: DragStartEvent) => {
    const { active } = event;
    const deal = active.data.current?.deal as KanbanDealCardDto;
    if (deal) {
      setActiveDeal(deal);
    }
  };

  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event;
    setActiveDeal(null);

    if (!over || !board) return;

    const dealId = active.id as string;
    const targetStageId = over.id as string;

    // Find source column and card
    let sourceCol: KanbanStageColumnDto | undefined;
    let card: KanbanDealCardDto | undefined;

    for (const col of board.columns) {
      const found = col.deals.find((d) => d.id === dealId);
      if (found) {
        sourceCol = col;
        card = found;
        break;
      }
    }

    if (!sourceCol || !card || sourceCol.stageId === targetStageId) {
      return; // No column change
    }

    const destCol = board.columns.find((c) => c.stageId === targetStageId);
    if (!destCol) return;

    // Preserve previous board state for rollback
    const previousBoard = JSON.parse(JSON.stringify(board)) as KanbanBoardDto;

    // Optimistic state update: move deal to destination column
    const updatedColumns = board.columns.map((col) => {
      if (col.stageId === sourceCol!.stageId) {
        const remainingDeals = col.deals.filter((d) => d.id !== dealId);
        return {
          ...col,
          deals: remainingDeals,
          dealsCount: remainingDeals.length,
          totalAmount: remainingDeals.reduce((sum, d) => sum + d.amount, 0),
          totalExpectedRevenue: remainingDeals.reduce((sum, d) => sum + d.expectedRevenue, 0),
        };
      }
      if (col.stageId === targetStageId) {
        const updatedDeal: KanbanDealCardDto = {
          ...card!,
          stageId: targetStageId,
          probability: destCol.probability,
          expectedRevenue: card!.amount * (destCol.probability / 100),
        };
        const newDeals = [updatedDeal, ...col.deals];
        return {
          ...col,
          deals: newDeals,
          dealsCount: newDeals.length,
          totalAmount: newDeals.reduce((sum, d) => sum + d.amount, 0),
          totalExpectedRevenue: newDeals.reduce((sum, d) => sum + d.expectedRevenue, 0),
        };
      }
      return col;
    });

    setBoard({
      ...board,
      columns: updatedColumns,
    });

    try {
      // Persist to real backend API and PostgreSQL
      const res = await dealService.updateDealStage(dealId, {
        stageId: targetStageId,
        notes: `Moved stage to ${destCol.stageName} via pipeline Kanban board.`,
      });

      if (!res.success) {
        // Rollback optimistic state
        setBoard(previousBoard);
        setError(res.message || 'Stage transition failed. Reverted.');
      } else {
        setNotification(`Opportunity "${card.title}" shifted to ${destCol.stageName}.`);
        setTimeout(() => setNotification(null), 3000);
      }
    } catch (err: unknown) {
      // Rollback on network/server error
      setBoard(previousBoard);
      setError(err instanceof Error ? err.message : 'Network error updating stage.');
    }
  };

  return (
    <div className="space-y-6">
      {/* Top Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white flex items-center gap-2">
            <Kanban className="w-6 h-6 text-blue-600" />
            <span>Sales Pipeline</span>
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Interactive drag-and-drop revenue pipeline with automated probability and expected revenue forecasting.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => navigate('/deals')}
            className="px-3 py-2 text-xs font-medium text-slate-700 dark:text-slate-200 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
          >
            Table View
          </button>

          <button
            onClick={() => {
              setTargetStageId(undefined);
              setIsCreateOpen(true);
            }}
            className="px-3.5 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg flex items-center gap-1.5 shadow-sm transition-colors"
          >
            <Plus className="w-4 h-4" />
            New Deal
          </button>
        </div>
      </div>

      {/* Notifications */}
      {notification && (
        <div className="p-3 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 rounded-lg text-xs text-emerald-800 dark:text-emerald-300 flex items-center gap-2">
          <CheckCircle className="w-4 h-4 shrink-0 text-emerald-600" />
          <span>{notification}</span>
        </div>
      )}

      {error && (
        <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {/* Board Summary Banner & Filters */}
      <div className="p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs flex flex-col md:flex-row md:items-center justify-between gap-4">
        {/* Pipeline Aggregates */}
        <div className="flex flex-wrap items-center gap-6 text-xs">
          <div>
            <div className="text-slate-400">Total Deals</div>
            <div className="text-base font-bold text-slate-900 dark:text-white">
              {board?.totalDealsCount ?? 0}
            </div>
          </div>
          <div className="h-8 w-px bg-slate-200 dark:bg-slate-700 hidden sm:block" />
          <div>
            <div className="text-slate-400">Total Pipeline Value</div>
            <div className="text-base font-bold text-slate-900 dark:text-white">
              ${board ? Number(board.totalPipelineValue).toLocaleString() : 0}
            </div>
          </div>
          <div className="h-8 w-px bg-slate-200 dark:bg-slate-700 hidden sm:block" />
          <div>
            <div className="text-slate-400">Forecasted Revenue</div>
            <div className="text-base font-bold text-emerald-600 dark:text-emerald-400">
              ${board ? Number(board.totalExpectedRevenue).toLocaleString() : 0}
            </div>
          </div>
        </div>

        {/* Filters */}
        <div className="flex flex-wrap items-center gap-2">
          <select
            value={assignedEmployeeId}
            onChange={(e) => setAssignedEmployeeId(e.target.value)}
            className="px-2.5 py-1.5 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
          >
            <option value="">All Sales Representatives</option>
            {employees.map((emp) => (
              <option key={emp.id} value={emp.id}>
                {emp.fullName}
              </option>
            ))}
          </select>

          <select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="px-2.5 py-1.5 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
          >
            <option value="">All Statuses</option>
            <option value="Open">Open Deals</option>
            <option value="Won">Won Deals</option>
            <option value="Lost">Lost Deals</option>
          </select>

          <button
            onClick={loadBoard}
            title="Refresh Board"
            className="p-2 border rounded-lg text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 border-slate-300 dark:border-slate-700 transition-colors"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin text-blue-500' : ''}`} />
          </button>
        </div>
      </div>

      {/* Kanban Board Horizontal Scroll Container */}
      {isLoading && !board ? (
        <div className="py-24 text-center text-slate-400">
          <RefreshCw className="w-8 h-8 animate-spin mx-auto mb-3 text-blue-500" />
          <p>Syncing sales pipeline data...</p>
        </div>
      ) : (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCorners}
          onDragStart={handleDragStart}
          onDragEnd={handleDragEnd}
        >
          <div className="flex gap-4 overflow-x-auto pb-4 pt-1">
            {board?.columns.map((column) => (
              <KanbanStageColumn
                key={column.stageId}
                column={column}
                onAddDeal={(stageId) => {
                  setTargetStageId(stageId);
                  setIsCreateOpen(true);
                }}
              />
            ))}
          </div>

          <DragOverlay>
            {activeDeal ? <KanbanDealCard deal={activeDeal} isOverlay /> : null}
          </DragOverlay>
        </DndContext>
      )}

      {/* Create Deal Modal */}
      <CreateDealModal
        isOpen={isCreateOpen}
        defaultStageId={targetStageId}
        onClose={() => {
          setIsCreateOpen(false);
          setTargetStageId(undefined);
        }}
        onSuccess={() => {
          setNotification('Deal created successfully.');
          loadBoard();
          setTimeout(() => setNotification(null), 3000);
        }}
      />
    </div>
  );
};
