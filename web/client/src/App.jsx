import React, { useState } from 'react';
import axios from 'axios';
import { Bell, LogIn, Send, User } from 'lucide-react';
import clsx from 'clsx';
import { twMerge } from 'tailwind-merge';

function cn(...inputs) {
  return twMerge(clsx(inputs));
}

const API_BASE = '/api';

export default function App() {
  const [token, setToken] = useState(localStorage.getItem('token') || '');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [notification, setNotification] = useState(null);

  const eventTypes = [
    { code: 'RANK_UP', label: 'Thăng hạng' },
    { code: 'VOUCHER_EXPIRING', label: 'Voucher sắp hết hạn' },
    { code: 'BIRTHDAY_GREETING', label: 'Quà sinh nhật' }
  ];

  const [isRegister, setIsRegister] = useState(false);
  const [fullName, setFullName] = useState('');

  const handleAuth = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      if (isRegister) {
        await axios.post(`${API_BASE}/users/register`, { 
          username: email, 
          email: email, 
          password,
          fullName: fullName || email.split('@')[0]
        });
        setIsRegister(false);
        setError('Đăng ký thành công! Vui lòng đăng nhập.');
        setLoading(false);
        return;
      }

      const res = await axios.post(`${API_BASE}/users/login`, { username: email, password });
      const accessToken = res.data.data.accessToken;
      setToken(accessToken);
      localStorage.setItem('token', accessToken);
    } catch (err) {
      setError(err.response?.data?.error?.message || err.response?.data?.message || 'Xác thực thất bại');
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    setToken('');
    localStorage.removeItem('token');
  };

  const simulateEvent = async (eventCode) => {
    setLoading(true);
    setNotification(null);
    try {
      const res = await axios.post(
        `${API_BASE}/notifications/simulate`,
        { eventTypeCode: eventCode },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setNotification(res.data);
    } catch (err) {
      alert(err.response?.data?.error?.message || err.response?.data?.message || 'Lỗi khi gọi API Simulate');
    } finally {
      setLoading(false);
    }
  };

  if (!token) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <div className="w-full max-w-md bg-white rounded-xl shadow-lg p-8">
          <div className="flex justify-center mb-8">
            <div className="h-12 w-12 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center">
              <User size={24} />
            </div>
          </div>
          <h2 className="text-2xl font-bold text-center mb-6">
            {isRegister ? 'Đăng ký Khách hàng' : 'Đăng nhập Khách hàng'}
          </h2>
          
          <form onSubmit={handleAuth} className="space-y-4">
            {isRegister && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Họ và tên</label>
                <input
                  type="text"
                  value={fullName}
                  onChange={e => setFullName(e.target.value)}
                  className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
                />
              </div>
            )}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Tài khoản (Email)</label>
              <input
                type="text"
                required
                value={email}
                onChange={e => setEmail(e.target.value)}
                className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Mật khẩu</label>
              <input
                type="password"
                required
                value={password}
                onChange={e => setPassword(e.target.value)}
                className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
              />
            </div>
            
            {error && <p className={clsx("text-sm", isRegister && error.includes('thành công') ? "text-green-600" : "text-red-500")}>{error}</p>}
            
            <button
              type="submit"
              disabled={loading}
              className="w-full bg-blue-600 text-white font-medium py-2 rounded-lg hover:bg-blue-700 transition flex items-center justify-center gap-2"
            >
              {loading ? 'Đang xử lý...' : <><LogIn size={18} /> {isRegister ? 'Đăng ký' : 'Đăng nhập'}</>}
            </button>
          </form>

          <div className="mt-4 text-center">
            <button 
              type="button" 
              onClick={() => setIsRegister(!isRegister)} 
              className="text-sm text-blue-600 hover:underline"
            >
              {isRegister ? 'Đã có tài khoản? Đăng nhập' : 'Chưa có tài khoản? Đăng ký ngay'}
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen pb-20">
      <header className="bg-white border-b sticky top-0 z-10">
        <div className="max-w-2xl mx-auto px-4 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2 text-blue-600 font-bold text-xl">
            <Bell size={24} />
            <span>Customer App</span>
          </div>
          <button onClick={handleLogout} className="text-sm text-gray-500 hover:text-gray-900">
            Đăng xuất
          </button>
        </div>
      </header>

      <main className="max-w-2xl mx-auto px-4 py-8 space-y-8">
        <section>
          <h2 className="text-lg font-bold mb-4">Giả lập Hành động Khách hàng</h2>
          <div className="grid grid-cols-2 gap-4">
            {eventTypes.map(type => (
              <button
                key={type.code}
                onClick={() => simulateEvent(type.code)}
                disabled={loading}
                className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-200 rounded-xl hover:border-blue-500 hover:shadow-sm transition group"
              >
                <Send size={18} className="text-gray-400 group-hover:text-blue-500" />
                <span className="font-medium text-gray-700 group-hover:text-blue-600">{type.label}</span>
              </button>
            ))}
          </div>
        </section>
      </main>

      {/* Notification Toast */}
      {notification && (
        <div className="fixed top-20 left-1/2 -translate-x-1/2 w-full max-w-sm z-50 animate-in slide-in-from-top-4 fade-in duration-300">
          <div className="bg-white rounded-2xl shadow-2xl overflow-hidden border border-gray-100">
            <div className="bg-blue-600 px-4 py-3 flex items-center gap-3">
              <div className="bg-white/20 p-1.5 rounded-lg">
                <Bell size={18} className="text-white" />
              </div>
              <p className="font-semibold text-white">Tin nhắn mới ({notification.channel})</p>
              <button 
                onClick={() => setNotification(null)}
                className="ml-auto text-white/70 hover:text-white"
              >
                ✕
              </button>
            </div>
            <div className="p-4">
              <h3 className="font-bold text-gray-900 mb-1">{notification.title}</h3>
              <p className="text-sm text-gray-600 whitespace-pre-wrap leading-relaxed">
                {notification.body}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
