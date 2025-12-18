// ETF 輸入元件 - 支援動態搜尋與驗證
const EtfInput = {
    name: 'EtfInput',
    template: `
        <div class="etf-input-container">
            <!-- 輸入框 -->
            <div class="input-group mb-3">
                <input 
                    type="text" 
                    class="form-control" 
                    v-model="inputSymbol"
                    @input="onInput"
                    @keyup.enter="addEtf"
                    placeholder="輸入 ETF 代碼（如 VTI, 0050.TW, AAPL）"
                    :disabled="selectedEtfs.length >= 5"
                />
                <button 
                    class="btn btn-primary" 
                    @click="addEtf"
                    :disabled="!canAdd"
                >
                    <span v-if="validating">
                        <span class="spinner-border spinner-border-sm me-1"></span>
                        驗證中...
                    </span>
                    <span v-else>新增</span>
                </button>
            </div>

            <!-- 驗證狀態 -->
            <div v-if="validationResult" class="validation-message mb-3">
                <div v-if="validationResult.isValid" class="alert alert-success py-2">
                    <i class="bi bi-check-circle-fill me-2"></i>
                    {{ validationResult.message }}
                </div>
                <div v-else class="alert alert-warning py-2">
                    <i class="bi bi-exclamation-triangle-fill me-2"></i>
                    {{ validationResult.message }}
                </div>
            </div>

            <!-- 建議清單 -->
            <div v-if="showSuggestions" class="suggestions-container mb-4">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <h6 class="mb-0">常用 ETF 建議</h6>
                    <button class="btn btn-sm btn-link" @click="showSuggestions = false">
                        收起
                    </button>
                </div>
                
                <div v-for="category in categories" :key="category" class="mb-3">
                    <div class="fw-bold text-muted small mb-1">{{ category }}</div>
                    <div class="d-flex flex-wrap gap-2">
                        <button 
                            v-for="etf in getSuggestionsByCategory(category)" 
                            :key="etf.symbol"
                            class="btn btn-sm btn-outline-primary"
                            @click="selectSuggestion(etf)"
                            :disabled="isAlreadySelected(etf.symbol)"
                        >
                            {{ etf.symbol }} - {{ etf.name }}
                        </button>
                    </div>
                </div>
            </div>

            <button 
                v-else 
                class="btn btn-sm btn-link mb-3 p-0" 
                @click="showSuggestions = true"
            >
                <i class="bi bi-lightbulb me-1"></i>
                顯示常用 ETF 建議
            </button>

            <!-- 已選 ETF 列表 -->
            <div v-if="selectedEtfs.length > 0" class="selected-etfs">
                <h6>已選擇的 ETF ({{ selectedEtfs.length }}/5)</h6>
                <div class="row g-2">
                    <div v-for="(etf, index) in selectedEtfs" :key="etf.symbol" class="col-md-6">
                        <div class="card etf-card">
                            <div class="card-body p-2">
                                <div class="d-flex justify-content-between align-items-start">
                                    <div class="flex-grow-1">
                                        <div class="fw-bold">{{ etf.symbol }}</div>
                                        <div class="small text-muted">{{ etf.name }}</div>
                                        <div class="small">
                                            <span class="badge bg-secondary me-1">{{ etf.market }}</span>
                                            <span v-if="etf.type" class="badge bg-info">{{ etf.type }}</span>
                                        </div>
                                    </div>
                                    <button 
                                        class="btn btn-sm btn-outline-danger"
                                        @click="removeEtf(index)"
                                        title="移除"
                                    >
                                        <i class="bi bi-x-lg"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,
    data() {
        return {
            inputSymbol: '',
            validating: false,
            validationResult: null,
            selectedEtfs: [],
            suggestions: [],
            showSuggestions: true,
            debounceTimer: null
        };
    },
    computed: {
        canAdd() {
            return this.validationResult?.isValid && 
                   this.selectedEtfs.length < 5 && 
                   !this.validating;
        },
        categories() {
            return [...new Set(this.suggestions.map(s => s.category))];
        }
    },
    methods: {
        async onInput() {
            // Debounce validation
            clearTimeout(this.debounceTimer);
            
            if (!this.inputSymbol.trim()) {
                this.validationResult = null;
                return;
            }

            this.debounceTimer = setTimeout(async () => {
                await this.validateSymbol();
            }, 500);
        },

        async validateSymbol() {
            const symbol = this.inputSymbol.trim();
            if (!symbol) return;

            this.validating = true;
            this.validationResult = null;

            try {
                const response = await fetch(`/api/etf/validate/${encodeURIComponent(symbol)}`);
                const result = await response.json();
                this.validationResult = result;
            } catch (error) {
                console.error('驗證失敗:', error);
                this.validationResult = {
                    isValid: false,
                    message: '驗證過程發生錯誤'
                };
            } finally {
                this.validating = false;
            }
        },

        async addEtf() {
            if (!this.canAdd) return;

            const etf = {
                symbol: this.validationResult.symbol,
                name: this.validationResult.name,
                type: this.validationResult.type,
                market: this.validationResult.market
            };

            this.selectedEtfs.push(etf);
            this.inputSymbol = '';
            this.validationResult = null;

            // Emit event for parent component
            this.$emit('etfs-changed', this.selectedEtfs);
        },

        removeEtf(index) {
            this.selectedEtfs.splice(index, 1);
            this.$emit('etfs-changed', this.selectedEtfs);
        },

        selectSuggestion(etf) {
            if (this.isAlreadySelected(etf.symbol)) return;

            this.selectedEtfs.push({
                symbol: etf.symbol,
                name: etf.name,
                type: 'ETF',
                market: etf.market
            });

            this.$emit('etfs-changed', this.selectedEtfs);
        },

        isAlreadySelected(symbol) {
            return this.selectedEtfs.some(e => e.symbol === symbol);
        },

        getSuggestionsByCategory(category) {
            return this.suggestions.filter(s => s.category === category);
        },

        async loadSuggestions() {
            try {
                const response = await fetch('/api/etf/suggestions');
                this.suggestions = await response.json();
            } catch (error) {
                console.error('載入建議清單失敗:', error);
            }
        },

        // 初始化預設 ETF
        async initializeDefaultEtfs() {
            const defaultSymbols = ['0050.TW', 'VTI'];
            
            for (const symbol of defaultSymbols) {
                try {
                    const response = await fetch(`/api/etf/validate/${encodeURIComponent(symbol)}`);
                    const result = await response.json();
                    
                    if (result.isValid) {
                        this.selectedEtfs.push({
                            symbol: result.symbol,
                            name: result.name,
                            type: result.type,
                            market: result.market
                        });
                    }
                } catch (error) {
                    console.error(`載入預設 ETF ${symbol} 失敗:`, error);
                }
            }

            this.$emit('etfs-changed', this.selectedEtfs);
        }
    },
    mounted() {
        this.loadSuggestions();
        this.initializeDefaultEtfs();
    }
};
