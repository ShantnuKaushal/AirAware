'use client';

import { useEffect, useState } from 'react';
import { Plane, Wind, Thermometer, AlertTriangle, CheckCircle, RefreshCw, AlertCircle } from 'lucide-react';

interface Flight {
  id: number;
  flightIata: string;
  airline: string;
  originAirport: string;
  destinationAirport: string;
  status: string;
  stressReport: {
    stressScore: number;
    temperatureC: number;
    windSpeedKph: number;
    maintenanceRecommendation: string;
  } | null;
}

export default function Dashboard() {
  const [flights, setFlights] = useState<Flight[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchFlights = async () => {
    setLoading(true);
    try {
      const res = await fetch('http://localhost:5077/api/Flights');
      const data = await res.json();
      setFlights(data);
    } catch (error) {
      console.error("Failed to fetch flights:", error);
    }
    setLoading(false);
  };

  const syncData = async () => {
    setLoading(true);
    await fetch('http://localhost:5077/api/Flights/sync', { method: 'POST' });
    await fetchFlights();
    setLoading(false);
  };

  useEffect(() => {
    fetchFlights();
  }, []);

  return (
    <div className="min-h-screen bg-neutral-950 text-neutral-100 p-8 font-sans">
      <div className="max-w-6xl mx-auto mb-10 flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold bg-gradient-to-r from-blue-400 to-cyan-300 bg-clip-text text-transparent">
            AirAware Command
          </h1>
          <p className="text-neutral-400 mt-1">Real-time Logistics & Weather Stress Analysis</p>
        </div>
        <button 
          onClick={syncData}
          disabled={loading}
          className="flex items-center gap-2 bg-blue-600 hover:bg-blue-500 text-white px-5 py-2.5 rounded-lg font-medium transition-all disabled:opacity-50"
        >
          <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          {loading ? 'Syncing...' : 'Sync Fleet'}
        </button>
      </div>

      <div className="max-w-6xl mx-auto grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {flights.slice(0, 9).map((flight) => (
          <div key={flight.id} className="bg-neutral-900 border border-neutral-800 rounded-xl p-5 hover:border-neutral-700 transition-colors shadow-xl">
            
            <div className="flex justify-between items-start mb-4">
              <div>
                <div className="flex items-center gap-2 text-lg font-bold text-white">
                  <Plane className="w-5 h-5 text-blue-400" />
                  {flight.flightIata}
                </div>
                <div className="text-sm text-neutral-400">
                    {flight.airline === "Unknown" || !flight.airline ? `Carrier: ${flight.flightIata}` : flight.airline}
                </div>
              </div>
              <span className={`px-2 py-1 rounded text-[10px] font-bold uppercase tracking-widest ${
                flight.status === 'active' ? 'bg-blue-500/10 text-blue-400 border border-blue-500/20' : 'bg-neutral-800 text-neutral-400'
              }`}>
                {flight.status}
              </span>
            </div>

            <div className="flex items-center gap-3 text-2xl font-mono text-neutral-300 mb-6">
              <span>{flight.originAirport}</span>
              <div className="h-px bg-neutral-700 flex-1 relative top-0.5"></div>
              <span className="text-white">{flight.destinationAirport}</span>
            </div>

            {flight.stressReport ? (
              <div className="bg-neutral-950/50 rounded-lg p-3 border border-neutral-800">
                <div className="flex justify-between items-center mb-3">
                  <span className="text-xs font-medium text-neutral-500 uppercase tracking-wider">Analysis Result</span>
                  
                  {flight.stressReport.stressScore >= 25 ? (
                    <div className="flex items-center gap-1 text-red-500 text-xs font-black animate-pulse">
                      <AlertTriangle className="w-3 h-3" /> CRITICAL
                    </div>
                  ) : flight.stressReport.stressScore >= 15 ? (
                    <div className="flex items-center gap-1 text-amber-500 text-xs font-bold">
                      <AlertCircle className="w-3 h-3" /> WARNING
                    </div>
                  ) : (
                    <div className="flex items-center gap-1 text-emerald-500 text-xs font-bold">
                      <CheckCircle className="w-3 h-3" /> HEALTHY
                    </div>
                  )}
                </div>

                <div className="grid grid-cols-2 gap-2 text-sm mb-3">
                    <div className="flex items-center gap-2 text-neutral-400">
                        <Thermometer className="w-4 h-4 text-neutral-500" />
                        {flight.stressReport.temperatureC.toFixed(1)}°C
                    </div>
                    <div className="flex items-center gap-2 text-neutral-400">
                        <Wind className="w-4 h-4 text-neutral-500" />
                        {flight.stressReport.windSpeedKph.toFixed(1)} kph
                    </div>
                </div>
                
                <div className="h-1.5 w-full bg-neutral-800 rounded-full overflow-hidden">
                    <div 
                        className={`h-full rounded-full transition-all duration-1000 ${
                            flight.stressReport.stressScore >= 25 ? 'bg-red-500' : 
                            flight.stressReport.stressScore >= 15 ? 'bg-amber-500' : 'bg-emerald-500'
                        }`}
                        style={{ width: `${Math.max(flight.stressReport.stressScore, 5)}%` }}
                    ></div>
                </div>
              </div>
            ) : (
              <div className="text-center py-4 text-neutral-600 text-sm italic">
                Awaiting Landing Analysis...
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}