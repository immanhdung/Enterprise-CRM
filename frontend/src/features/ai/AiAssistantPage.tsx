import React, { useState, useEffect, useRef } from 'react';
import { AiNavigation } from './AiNavigation';
import { aiService } from '../../services/aiService';
import type { AiConversation, AiConversationMessage } from '../../types/ai';
import { Bot, Send, Plus, Trash2, User, Sparkles, MessageSquare } from 'lucide-react';

export const AiAssistantPage: React.FC = () => {
  const [conversations, setConversations] = useState<AiConversation[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);
  const [messages, setMessages] = useState<AiConversationMessage[]>([]);
  const [inputText, setInputText] = useState('');
  const [sending, setSending] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const fetchConversations = async () => {
    try {
      const convs = await aiService.getConversations();
      setConversations(convs);
      if (convs.length > 0 && !activeConversationId) {
        setActiveConversationId(convs[0].id);
      }
    } catch (err) {
      console.error('Failed to load conversations:', err);
    }
  };

  const fetchMessages = async (conversationId: string) => {
    try {
      const msgs = await aiService.getConversationMessages(conversationId);
      setMessages(msgs);
    } catch (err) {
      console.error('Failed to load messages:', err);
    }
  };

  useEffect(() => {
    fetchConversations();
  }, []);

  useEffect(() => {
    if (activeConversationId) {
      fetchMessages(activeConversationId);
    } else {
      setMessages([]);
    }
  }, [activeConversationId]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, sending]);

  const handleSendMessage = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!inputText.trim() || sending) return;

    const userText = inputText.trim();
    setInputText('');

    // Optimistic user message
    const tempUserMsg: AiConversationMessage = {
      id: String(Date.now()),
      conversationId: activeConversationId || '',
      role: 'user',
      content: userText,
      tokensUsed: 0,
      createdAtUtc: new Date().toISOString(),
    };
    setMessages((prev) => [...prev, tempUserMsg]);
    setSending(true);

    try {
      const res = await aiService.sendChatMessage({
        conversationId: activeConversationId,
        message: userText,
      });

      if (!activeConversationId) {
        setActiveConversationId(res.conversationId);
        fetchConversations();
      }

      setMessages((prev) => [...prev, res.message]);
    } catch (err: any) {
      alert(`Lỗi AI Assistant: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setSending(false);
    }
  };

  const handleNewChat = () => {
    setActiveConversationId(null);
    setMessages([]);
  };

  const handleDeleteConversation = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!window.confirm('Xóa đoạn hội thoại này?')) return;
    try {
      await aiService.deleteConversation(id);
      if (activeConversationId === id) {
        handleNewChat();
      }
      fetchConversations();
    } catch (err) {
      console.error('Error deleting conversation:', err);
    }
  };

  const quickPrompts = [
    'Dự báo doanh thu thực thu tháng này và quý tới?',
    'Top 5 cơ hội bán hàng (Deals) có giá trị cao nhất?',
    'Phân tích hiệu suất và điểm mạnh/yếu của đội sales?',
    'Danh sách hợp đồng sắp đến hạn thanh toán hoặc gia hạn?',
  ];

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Trợ lý AI Doanh nghiệp (Enterprise AI Copilot)</h1>
          <p className="text-sm text-gray-500">
            Trò chuyện thông minh, phân tích số liệu thời gian thực và hỗ trợ ra quyết định kinh doanh
          </p>
        </div>
      </div>

      <AiNavigation />

      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden h-[680px]">
        {/* Sidebar History */}
        <div className="md:col-span-1 border-r border-gray-200 flex flex-col bg-gray-50">
          <div className="p-3 border-b border-gray-200">
            <button
              onClick={handleNewChat}
              className="w-full flex items-center justify-center gap-2 bg-purple-600 hover:bg-purple-700 text-white px-3 py-2 rounded-lg text-sm font-medium transition-colors shadow-sm"
            >
              <Plus className="w-4 h-4" />
              Cuộc trò chuyện mới
            </button>
          </div>

          <div className="flex-1 overflow-y-auto p-2 space-y-1">
            {conversations.length === 0 ? (
              <div className="p-4 text-center text-xs text-gray-400">Chưa có lịch sử trò chuyện.</div>
            ) : (
              conversations.map((c) => (
                <div
                  key={c.id}
                  onClick={() => setActiveConversationId(c.id)}
                  className={`flex items-center justify-between p-2.5 rounded-lg cursor-pointer text-xs transition-colors ${
                    activeConversationId === c.id
                      ? 'bg-purple-100 text-purple-900 font-medium'
                      : 'hover:bg-gray-200 text-gray-700'
                  }`}
                >
                  <div className="flex items-center gap-2 truncate">
                    <MessageSquare className="w-4 h-4 shrink-0 text-gray-400" />
                    <span className="truncate">{c.title}</span>
                  </div>
                  <button
                    onClick={(e) => handleDeleteConversation(c.id, e)}
                    className="p-1 text-gray-400 hover:text-red-500"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Chat Stream & Input */}
        <div className="md:col-span-3 flex flex-col h-full bg-white">
          {/* Messages Stream */}
          <div className="flex-1 overflow-y-auto p-6 space-y-4">
            {messages.length === 0 ? (
              <div className="h-full flex flex-col items-center justify-center text-center p-6">
                <div className="w-16 h-16 rounded-2xl bg-purple-100 text-purple-600 flex items-center justify-center mb-4">
                  <Bot className="w-8 h-8" />
                </div>
                <h3 className="text-lg font-bold text-gray-900">Enterprise AI Copilot sẵn sàng hỗ trợ</h3>
                <p className="text-sm text-gray-500 max-w-md mt-1 mb-6">
                  Bạn có thể đặt câu hỏi về doanh thu, khách hàng tiềm năng, hiệu suất đội ngũ hoặc chọn các gợi ý bên dưới:
                </p>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-2 max-w-lg w-full">
                  {quickPrompts.map((prompt, idx) => (
                    <button
                      key={idx}
                      onClick={() => {
                        setInputText(prompt);
                      }}
                      className="text-left text-xs bg-gray-50 hover:bg-purple-50 hover:border-purple-200 border border-gray-200 p-3 rounded-lg text-gray-700 transition-colors flex items-start gap-2"
                    >
                      <Sparkles className="w-3.5 h-3.5 text-purple-600 shrink-0 mt-0.5" />
                      <span>{prompt}</span>
                    </button>
                  ))}
                </div>
              </div>
            ) : (
              messages.map((m) => {
                const isAssistant = m.role === 'assistant';
                return (
                  <div
                    key={m.id}
                    className={`flex items-start gap-3 ${isAssistant ? '' : 'flex-row-reverse'}`}
                  >
                    <div
                      className={`w-8 h-8 rounded-full flex items-center justify-center shrink-0 ${
                        isAssistant ? 'bg-purple-600 text-white' : 'bg-blue-600 text-white'
                      }`}
                    >
                      {isAssistant ? <Bot className="w-4 h-4" /> : <User className="w-4 h-4" />}
                    </div>
                    <div
                      className={`max-w-2xl rounded-2xl p-4 text-sm leading-relaxed shadow-sm whitespace-pre-wrap ${
                        isAssistant
                          ? 'bg-gray-50 border border-gray-200 text-gray-800'
                          : 'bg-blue-600 text-white'
                      }`}
                    >
                      {m.content}
                      {m.modelUsed && (
                        <div className="text-[10px] text-gray-400 mt-2 text-right">
                          Model: {m.modelUsed} ({m.tokensUsed} tokens)
                        </div>
                      )}
                    </div>
                  </div>
                );
              })
            )}

            {sending && (
              <div className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-full bg-purple-600 text-white flex items-center justify-center shrink-0">
                  <Bot className="w-4 h-4" />
                </div>
                <div className="bg-gray-50 border border-gray-200 p-3 rounded-2xl text-xs text-gray-500 flex items-center gap-2">
                  <div className="w-2 h-2 rounded-full bg-purple-600 animate-pulse" />
                  <span>Trợ lý AI đang tổng hợp số liệu thực tế...</span>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Chat Input Bar */}
          <div className="p-4 border-t border-gray-200 bg-gray-50">
            <form onSubmit={handleSendMessage} className="flex gap-2">
              <input
                type="text"
                value={inputText}
                onChange={(e) => setInputText(e.target.value)}
                placeholder="Nhập câu hỏi bằng tiếng Việt hoặc tiếng Anh..."
                className="flex-1 px-4 py-2.5 bg-white border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-purple-500"
              />
              <button
                type="submit"
                disabled={sending || !inputText.trim()}
                className="px-5 py-2.5 bg-purple-600 hover:bg-purple-700 disabled:opacity-50 text-white rounded-lg text-sm font-medium transition-colors flex items-center gap-1.5 shadow-sm"
              >
                <Send className="w-4 h-4" />
                <span>Gửi</span>
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};
