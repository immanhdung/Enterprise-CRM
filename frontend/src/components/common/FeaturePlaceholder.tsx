import React from 'react';
import { Construction } from 'lucide-react';

interface FeaturePlaceholderProps {
  title: string;
  description: string;
  phase: string;
}

export const FeaturePlaceholder: React.FC<FeaturePlaceholderProps> = ({ title, description, phase }) => {
  return (
    <div className="bg-white rounded-xl border border-slate-200 p-12 text-center shadow-xs max-w-2xl mx-auto my-12">
      <div className="w-12 h-12 rounded-full bg-blue-50 border border-blue-100 flex items-center justify-center text-blue-600 mx-auto mb-4">
        <Construction className="w-6 h-6" />
      </div>
      <h2 className="text-xl font-bold text-slate-900">{title}</h2>
      <p className="text-sm text-slate-500 mt-2 max-w-md mx-auto">{description}</p>
      <div className="mt-6 inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-slate-100 text-slate-700">
        Roadmap: {phase}
      </div>
    </div>
  );
};
