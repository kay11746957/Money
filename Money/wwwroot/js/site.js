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
            if (newValue <= 0) {
                this.errors.monthlyInvestment = '月投入金額必須大於 0 元';
            } else {
                this.errors.monthlyInvestment = null;
            }
        },
        'form.years'(newValue) {
            if (newValue < 1) {
                this.errors.years = '投資年數至少 1 年';
            } else if (newValue > 50) {
                this.errors.years = '投資年數最多 50 年';
            } else {
                this.errors.years = null;
            }
        },
    },
    methods: {
        async calculate() {
            if (!this.isValid) return;

            this.loading = true;
            this.result = null;

            try {
                const response = await fetch('/Home/Calculate', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(this.form),
                });

                if (!response.ok) {
                    // Handle validation errors from backend if needed
                    console.error('Calculation failed');
                    return;
                }

                this.result = await response.json();
                
                // Use nextTick to ensure the DOM is updated before rendering the chart
                this.$nextTick(() => {
                    this.renderChart();
                });

            } catch (error) {
                console.error('An error occurred:', error);
            } finally {
                this.loading = false;
            }
        },
        renderChart() {
            const ctx = document.getElementById('dca-chart').getContext('2d');
            const labels = this.result.yearlyBreakdown.map(d => d.year);
            const data = this.result.yearlyBreakdown.map(d => d.value);

            if (this.chart) {
                this.chart.destroy();
            }

            this.chart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: '資產總值',
                        data: data,
                        borderColor: '#7c3aed',
                        backgroundColor: 'rgba(124, 58, 237, 0.1)',
                        fill: true,
                        tension: 0.4,
                        pointRadius: 0,
                        pointHoverRadius: 6,
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function(value) {
                                    if (value >= 1000000) return (value / 1000000) + 'M';
                                    if (value >= 1000) return (value / 1000) + 'K';
                                    return value;
                                }
                            }
                        }
                    },
                    plugins: {
                        tooltip: {
                            callbacks: {
                                label: function(context) {
                                    let label = context.dataset.label || '';
                                    if (label) {
                                        label += ': ';
                                    }
                                    if (context.parsed.y !== null) {
                                        label += new Intl.NumberFormat('zh-TW', { style: 'currency', currency: 'TWD', minimumFractionDigits: 0 }).format(context.parsed.y);
                                    }
                                    return label;
                                }
                            }
                        }
                    }
                }
            });
        },
        formatCurrency(value) {
            return new Intl.NumberFormat('zh-TW', { style: 'currency', currency: 'TWD', minimumFractionDigits: 0 }).format(value);
        }
    },
    mounted() {
        // Add a link to Bootstrap Icons
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css';
        document.head.appendChild(link);
    }
}).mount('#app');