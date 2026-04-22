'use client';

import { useEffect, useState } from 'react';
import {
  AlertCircle,
  AlertTriangle,
  CheckCircle,
  Plane,
  RefreshCw,
  Thermometer,
  Wind,
} from 'lucide-react';

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
      const response = await fetch('http://localhost:5077/api/Flights');
      const data = await response.json();
      setFlights(data);
    } catch (error) {
      console.error('Failed to fetch flights:', error);
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
    <div className="min-h-screen bg-neutral-950 p-8 font-sans text-neutral-100">
      <div className="mx-auto mb-10 flex max-w-6xl items-center justify-between">
        <div>
          <h1 className="bg-gradient-to-r from-blue-400 to-cyan-300 bg-clip-text text-3xl font-bold text-transparent">
            AirAware Command
          </h1>
          <p className="mt-1 text-neutral-400">Real-time Logistics & Weather Stress Analysis</p>
        </div>
        <button
          onClick={syncData}
          disabled={loading}
          className="flex items-center gap-2 rounded-lg bg-blue-600 px-5 py-2.5 font-medium text-white transition-all hover:bg-blue-500 disabled:opacity-50"
        >
          <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
          {loading ? 'Syncing...' : 'Sync Fleet'}
        </button>
      </div>

      <div className="mx-auto grid max-w-6xl grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
        {flights.slice(0, 9).map((flight) => (
          <div
            key={flight.id}
            className="rounded-xl border border-neutral-800 bg-neutral-900 p-5 shadow-xl transition-colors hover:border-neutral-700"
          >
            <div className="mb-4 flex items-start justify-between">
              <div>
                <div className="flex items-center gap-2 text-lg font-bold text-white">
                  <Plane className="h-5 w-5 text-blue-400" />
                  {flight.flightIata}
                </div>
                <div className="text-sm text-neutral-400">
                  {flight.airline === 'Unknown' || !flight.airline
                    ? `Carrier: ${flight.flightIata}`
                    : flight.airline}
                </div>
              </div>
              <span
                className={`rounded px-2 py-1 text-[10px] font-bold uppercase tracking-widest ${
                  flight.status === 'active'
                    ? 'border border-blue-500/20 bg-blue-500/10 text-blue-400'
                    : 'bg-neutral-800 text-neutral-400'
                }`}
              >
                {flight.status}
              </span>
            </div>

            <div className="mb-6 flex items-center gap-3 font-mono text-2xl text-neutral-300">
              <span>{flight.originAirport}</span>
              <div className="relative top-0.5 h-px flex-1 bg-neutral-700"></div>
              <span className="text-white">{flight.destinationAirport}</span>
            </div>

            {flight.stressReport ? (
              <div className="rounded-lg border border-neutral-800 bg-neutral-950/50 p-3">
                <div className="mb-3 flex items-center justify-between">
                  <span className="text-xs font-medium uppercase tracking-wider text-neutral-500">
                    Analysis Result
                  </span>

                  {flight.stressReport.stressScore >= 25 ? (
                    <div className="flex items-center gap-1 animate-pulse text-xs font-black text-red-500">
                      <AlertTriangle className="h-3 w-3" /> CRITICAL
                    </div>
                  ) : flight.stressReport.stressScore >= 15 ? (
                    <div className="flex items-center gap-1 text-xs font-bold text-amber-500">
                      <AlertCircle className="h-3 w-3" /> WARNING
                    </div>
                  ) : (
                    <div className="flex items-center gap-1 text-xs font-bold text-emerald-500">
                      <CheckCircle className="h-3 w-3" /> HEALTHY
                    </div>
                  )}
                </div>

                <div className="mb-3 grid grid-cols-2 gap-2 text-sm">
                  <div className="flex items-center gap-2 text-neutral-400">
                    <Thermometer className="h-4 w-4 text-neutral-500" />
                    {flight.stressReport.temperatureC.toFixed(1)} deg C
                  </div>
                  <div className="flex items-center gap-2 text-neutral-400">
                    <Wind className="h-4 w-4 text-neutral-500" />
                    {flight.stressReport.windSpeedKph.toFixed(1)} kph
                  </div>
                </div>

                <div className="h-1.5 w-full overflow-hidden rounded-full bg-neutral-800">
                  <div
                    className={`h-full rounded-full transition-all duration-1000 ${
                      flight.stressReport.stressScore >= 25
                        ? 'bg-red-500'
                        : flight.stressReport.stressScore >= 15
                          ? 'bg-amber-500'
                          : 'bg-emerald-500'
                    }`}
                    style={{ width: `${Math.max(flight.stressReport.stressScore, 5)}%` }}
                  ></div>
                </div>
              </div>
            ) : (
              <div className="py-4 text-center text-sm italic text-neutral-600">
                Awaiting Landing Analysis...
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
