const { createApp } = Vue;

createApp({
    data() {
        return {
            searchQuery: '',
            showDropdown: false,
            selectedEtfs: [],
            loading: false,
            result: null,
            chart: null,
            backtestMode: 'compare', // 'compare' or 'portfolio'

            // 回測參數
            params: {
                period: '10',
                investmentMode: 'dca',
                amount: 10000,
                reinvestDividends: true,
            },

            // 投資組合設定
            portfolioItems: [
                { symbol: '0050', weight: 60 },
                { symbol: 'VTI', weight: 40 },
            ],

            // ETF 清單
            etfList: [
                // 台股 ETF
                { symbol: '0050', name: '元大台灣50', market: 'TW' },
                { symbol: '0056', name: '元大高股息', market: 'TW' },
                { symbol: '006208', name: '富邦台50', market: 'TW' },
                { symbol: '00878', name: '國泰永續高股息', market: 'TW' },
                { symbol: '00692', name: '富邦公司治理', market: 'TW' },
                { symbol: '00679B', name: '元大美債20年', market: 'TW' },

                // 美股 ETF
                { symbol: 'VTI', name: 'Vanguard 全美股市', market: 'US' },
                { symbol: 'VOO', name: 'Vanguard S&P 500', market: 'US' },
                { symbol: 'VT', name: 'Vanguard 全世界股市', market: 'US' },
                { symbol: 'QQQ', name: 'Invesco 納斯達克100', market: 'US' },
                { symbol: 'VWO', name: 'Vanguard 新興市場', market: 'US' },
                { symbol: 'VEA', name: 'Vanguard 已開發市場', market: 'US' },
                { symbol: 'BND', name: 'Vanguard 美國總債券', market: 'US' },
            ],
        };
    },

    computed: {
        filteredEtfs() {
            if (!this.searchQuery) {
                return this.etfList.filter(etf => !this.isSelected(etf));
            }
            const query = this.searchQuery.toLowerCase();
            return this.etfList.filter(etf =>
                !this.isSelected(etf) &&
                (etf.symbol.toLowerCase().includes(query) ||
                    etf.name.toLowerCase().includes(query))
            );
        },

        filteredTwEtfs() {
            return this.filteredEtfs.filter(etf => etf.market === 'TW');
        },

        filteredUsEtfs() {
            return this.filteredEtfs.filter(etf => etf.market === 'US');
        },

        totalWeight() {
            return this.portfolioItems.reduce((sum, item) => sum + (item.weight || 0), 0);
        },
    },

    methods: {
        isSelected(etf) {
            return this.selectedEtfs.some(e => e.symbol === etf.symbol);
        },

        selectEtf(etf) {
            if (!this.isSelected(etf) && this.selectedEtfs.length < 5) {
                this.selectedEtfs.push(etf);
            }
            this.searchQuery = '';
            this.showDropdown = false;
        },

        removeEtf(etf) {
            this.selectedEtfs = this.selectedEtfs.filter(e => e.symbol !== etf.symbol);
        },

        formatCurrency(value) {
            return new Intl.NumberFormat('zh-TW', {
                style: 'currency',
                currency: 'TWD',
                minimumFractionDigits: 0
            }).format(value);
        },

        isWinner(etfResult) {
            if (!this.result || !this.result.results || this.result.results.length <= 1) {
                return false;
            }
            const maxReturn = Math.max(...this.result.results.map(r => r.totalReturnPercent));
            return etfResult.totalReturnPercent === maxReturn;
        },

        // 投資組合相關方法
        addPortfolioItem() {
            if (this.portfolioItems.length < 5) {
                this.portfolioItems.push({ symbol: '', weight: 0 });
            }
        },

        removePortfolioItem(index) {
            if (this.portfolioItems.length > 1) {
                this.portfolioItems.splice(index, 1);
            }
        },

        // 分享功能
        async shareResult() {
            const shareData = {
                mode: this.backtestMode,
                etfs: this.backtestMode === 'compare'
                    ? this.selectedEtfs.map(e => e.symbol)
                    : this.portfolioItems.filter(i => i.symbol).map(i => ({ s: i.symbol, w: i.weight })),
                period: this.params.period,
                mode: this.params.investmentMode,
                amount: this.params.amount,
            };

            const encoded = btoa(JSON.stringify(shareData));
            const shareUrl = `${window.location.origin}${window.location.pathname}?config=${encoded}`;

            try {
                await navigator.clipboard.writeText(shareUrl);
                alert('🔗 分享連結已複製到剪貼簿！');
            } catch (err) {
                // Fallback
                prompt('複製以下連結分享:', shareUrl);
            }
        },

        async startBacktest() {
            if (this.selectedEtfs.length === 0) return;

            this.loading = true;
            this.result = null;

            try {
                const response = await fetch('/api/backtest', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        symbols: this.selectedEtfs.map(e => e.symbol),
                        period: parseInt(this.params.period),
                        investmentMode: this.params.investmentMode,
                        amount: this.params.amount,
                        reinvestDividends: this.params.reinvestDividends,
                    }),
                });

                if (!response.ok) {
                    throw new Error('回測 API 呼叫失敗');
                }

                const data = await response.json();
                this.result = data;

                console.log('回測完成:', data);

                // 渲染圖表
                this.$nextTick(() => {
                    this.renderChart();
                });

            } catch (error) {
                console.error('回測失敗:', error);
                alert('回測失敗，請稍後再試');
            } finally {
                this.loading = false;
            }
        },

        renderChart() {
            if (!this.result || !this.result.results.length) return;

            const ctx = document.getElementById('performance-chart');
            if (!ctx) return;

            // 銷毀舊圖表
            if (this.chart) {
                this.chart.destroy();
            }

            const colors = [
                { line: '#7c3aed', bg: 'rgba(124, 58, 237, 0.1)' },
                { line: '#10b981', bg: 'rgba(16, 185, 129, 0.1)' },
                { line: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
                { line: '#3b82f6', bg: 'rgba(59, 130, 246, 0.1)' },
                { line: '#ef4444', bg: 'rgba(239, 68, 68, 0.1)' },
            ];

            const datasets = this.result.results.map((r, index) => {
                const color = colors[index % colors.length];
                return {
                    label: `${r.symbol} - ${r.name}`,
                    data: r.performanceData.map(p => ({
                        x: new Date(p.date),
                        y: p.cumulativeReturn
                    })),
                    borderColor: color.line,
                    backgroundColor: color.bg,
                    fill: true,
                    tension: 0.3,
                    pointRadius: 0,
                    pointHoverRadius: 6,
                };
            });

            this.chart = new Chart(ctx, {
                type: 'line',
                data: { datasets },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    interaction: {
                        intersect: false,
                        mode: 'index',
                    },
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        tooltip: {
                            callbacks: {
                                label: (context) => {
                                    return `${context.dataset.label}: ${context.parsed.y.toFixed(2)}%`;
                                }
                            }
                        }
                    },
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                unit: 'month',
                                displayFormats: {
                                    month: 'yyyy/MM'
                                }
                            },
                            title: {
                                display: true,
                                text: '日期'
                            }
                        },
                        y: {
                            title: {
                                display: true,
                                text: '累積報酬率 (%)'
                            },
                            ticks: {
                                callback: (value) => value + '%'
                            }
                        }
                    }
                }
            });
        },
    },

    mounted() {
        // 預設選擇 0050 和 VTI
        const defaultEtfs = ['0050', 'VTI'];
        defaultEtfs.forEach(symbol => {
            const etf = this.etfList.find(e => e.symbol === symbol);
            if (etf) {
                this.selectedEtfs.push(etf);
            }
        });

        // 點擊外部關閉下拉選單
        document.addEventListener('click', (e) => {
            if (!e.target.closest('#etf-selection')) {
                this.showDropdown = false;
            }
        });
    },
}).mount('#app');
