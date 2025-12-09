const { createApp } = Vue;

createApp({
    data() {
        return {
            form: {
                monthlyInvestment: 10000,
                annualRate: 6,
                years: 20,
            },
            annualRates: [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 15, 20],
            errors: {
                monthlyInvestment: null,
                years: null,
            },
            loading: false,
            result: null,
            chart: null,
        };
    },
    computed: {
        isValid() {
            return !this.errors.monthlyInvestment && !this.errors.years;
        },
    },
    watch: {
        'form.monthlyInvestment'(newValue) {
            this.validateMonthlyInvestment(newValue);
        },
        'form.years'(newValue) {
            this.validateYears(newValue);
        },
    },
    methods: {
        async calculate() {
            if (!this.isValid) return;

            this.loading = true;

            try {
                const response = await fetch('/Calculator/Calculate', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(this.form),
                });

                if (response.ok) {
                    const data = await response.json();
                    this.result = data;
                    this.$nextTick(() => {
                        this.renderChart(data.yearlyBreakdown);
                    });
                } else {
                    console.error('Calculation failed');
                    alert('計算失敗，請稍後再試');
                }
            } catch (error) {
                console.error('Error:', error);
                alert('發生錯誤，請稍後再試');
            } finally {
                this.loading = false;
            }
        },
        formatCurrency(value) {
            return new Intl.NumberFormat('zh-TW', { style: 'currency', currency: 'TWD', minimumFractionDigits: 0 }).format(value);
        },
        validateMonthlyInvestment(value) {
            if (value <= 0) {
                this.errors.monthlyInvestment = '月投入金額必須大於 0 元';
            } else {
                this.errors.monthlyInvestment = null;
            }
        },
        validateYears(value) {
            if (value < 1) {
                this.errors.years = '投資年數至少 1 年';
            } else if (value > 50) {
                this.errors.years = '投資年數最多 50 年';
            } else {
                this.errors.years = null;
            }
        },
        renderChart(yearlyBreakdown) {
            const ctx = document.getElementById('dca-chart');
            if (!ctx) return;

            // Destroy existing chart if it exists
            if (this.chart) {
                this.chart.destroy();
            }

            const labels = yearlyBreakdown.map(item => `第 ${item.year} 年`);
            const data = yearlyBreakdown.map(item => item.value);

            this.chart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: '累積資產',
                        data: data,
                        borderColor: '#7c3aed',
                        backgroundColor: 'rgba(124, 58, 237, 0.1)',
                        fill: true,
                        tension: 0.4,
                        pointBackgroundColor: '#7c3aed',
                        pointBorderColor: '#ffffff',
                        pointBorderWidth: 2,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false,
                        },
                        tooltip: {
                            callbacks: {
                                label: (context) => {
                                    const value = context.raw;
                                    return `資產: ${new Intl.NumberFormat('zh-TW').format(value)} 元`;
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: (value) => {
                                    if (value >= 1000000) {
                                        return (value / 1000000).toFixed(1) + 'M';
                                    } else if (value >= 1000) {
                                        return (value / 1000).toFixed(0) + 'K';
                                    }
                                    return value;
                                }
                            }
                        },
                        x: {
                            ticks: {
                                maxTicksLimit: 10,
                            }
                        }
                    }
                }
            });
        }
    },
    mounted() {
        // Validate initial values
        this.validateMonthlyInvestment(this.form.monthlyInvestment);
        this.validateYears(this.form.years);
    }
}).mount('#app');
