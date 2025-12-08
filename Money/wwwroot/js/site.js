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
                const response = await fetch('/Home/Calculate', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(this.form),
                });

                if (response.ok) {
                    const data = await response.json();
                    this.result = data;

                    // Wait for DOM to update, then delay chart rendering to ensure canvas is fully ready
                    this.$nextTick(() => {
                        // Additional delay to ensure canvas is fully mounted and stable
                        setTimeout(() => {
                            this.renderChart();
                        }, 500);
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
        renderChart() {
            // Destroy existing chart first
            if (this.chart) {
                try {
                    this.chart.destroy();
                    this.chart = null;
                } catch (e) {
                    console.warn('Error destroying chart:', e);
                }
            }

            const canvas = document.getElementById('dca-chart');
            if (!canvas) {
                console.warn('Canvas element not found, skipping chart render');
                return;
            }

            // Verify canvas is in DOM and accessible
            if (!canvas.ownerDocument || !document.body.contains(canvas)) {
                console.warn('Canvas not in DOM, skipping chart render');
                return;
            }

            let ctx;
            try {
                ctx = canvas.getContext('2d');
                if (!ctx) {
                    console.warn('Cannot get canvas context, skipping chart render');
                    return;
                }
            } catch (e) {
                console.warn('Error getting canvas context:', e);
                return;
            }

            const labels = this.result.yearlyBreakdown.map(d => d.year);
            const data = this.result.yearlyBreakdown.map(d => d.value);

            try {
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
                                    callback: function (value) {
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
                                    label: function (context) {
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
            } catch (e) {
                console.error('Error creating chart:', e);
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
        }
    },
    mounted() {
        // Validate initial values
        this.validateMonthlyInvestment(this.form.monthlyInvestment);
        this.validateYears(this.form.years);

        // Add a link to Bootstrap Icons
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css';
        document.head.appendChild(link);

        // Manually bind button click event (workaround for Vue event binding issue)
        this.$nextTick(() => {
            const button = document.querySelector('button[type="submit"]');
            if (button) {
                button.onclick = (e) => {
                    e.preventDefault();
                    this.calculate();
                };
            }
        });
    }
}).mount('#app');